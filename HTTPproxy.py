# Place your imports here
import signal
from optparse import OptionParser
from socket import *
import sys
import re
import threading

#Global server socket
serverSocket = socket(AF_INET,SOCK_STREAM)
#Global thread lock
lock = threading.Lock()
#Global dictionary for caching
#Key is hostname and path
#Value is a cached object found at that hostname and path
cacheList={}
#Global list for domain blocking where each each object is the hostname of a blocked site
#Hostnames in the list are blocked by the proxy
blockList={}
#Global boolean variable to track if caching is enabled or disabled
cacheEnable = False
#Global boolean variable to track if domain blocking is enabled or not
blockEnable = False

# Signal handler for pressing ctrl-c
def ctrl_c_pressed(signal, frame):
    serverSocket.close()
    sys.exit(0)

#Caching is enabled
#Returns - void
def enableCache():
    lock.acquire()
    global cacheEnable
    cacheEnable=True
    lock.release()

#Caching is disabled
#Returns - void
def disableCache():
    lock.acquire()
    global cacheEnable
    cacheEnable=False
    lock.release()
    
#Domain blocking is enabled
#Returns - void
def enableBlock():
    lock.acquire()
    global blockEnable
    blockEnable=True
    lock.release()
    
#Domain blocking is disabled
#Returns - void
def disableBlock():
    lock.acquire()
    global blockEnable
    blockEnable=False
    lock.release()

#Empties list of cached object
#Returns - void
def emptyCacheList():
    lock.acquire()
    global cacheList
    cacheList={}
    lock.release()

#Empties list of blocked sites
#Returns - void
def emptyBlockList():
    lock.acquire()
    global blockList
    blockList={}
    lock.release()

#Adds a cached object to the global list of cached objects
#@param url - the url of a object
#@param object - the object found at url
#Returns - void
def addToCacheList(url, object):
    lock.acquire()
    global cacheList
    cacheList.update({url: object})
    lock.release()

#Removes a cached object from the global list of cached objects based on its url
#@param url - the url of a object
#Returns - void
def removeFromCacheList(url):
    lock.acquire()
    global cacheList
    cacheList.pop(url)
    lock.release()
    
#Adds a domain to the global list of blocked domains
#@param url - the url of the blocked domain
#Returns - void
def addToBlockList(host):
    lock.acquire()
    global blockList
    blockList.update({host: 1})
    lock.release()

#Removes a domain from the global list of blocked domains
#@param host - the hostname of the blocked domain
#Returns - void
def removeFromBlockList(host):
    lock.acquire()
    global blockList
    blockList.pop(host)
    lock.release()

#Checks if a domain is in the list of blocked domains
#@param host - the hostname of the domain
#Returns - true if the domain is in the list, false otherwise
def inBlockList(host):
    global blockList
    if host in blockList:
        return True
    for x in blockList:
        if x in host or host in x:
            return True
    return False        

def printCacheEnable():
    print('cache enabled: ')
    print(cacheEnable)

def printBlockEnable():
    print('block enabled: ')
    print(blockEnable)

def printCacheList():
    print('cachelist: ')
    print(cacheList)
    print('\n')

def printBlockList():
    print('blocklist: ')
    print(blockList)
    print('\n')

#Finds the date from the 'If-Modified-Since' header of object received from a remote server
#@param message - the message received from a remote server
#Returns - the date from the 'If-Modified-Since' header
def getDate(message):
    date=b''
    messageLines=message.split(b'\r\n')
    for messageLine in messageLines:
        messageLineParts=messageLine.split(b'Last-Modified:')
        if len(messageLineParts)==2 and messageLineParts[0]==b'':
            date=messageLineParts[1]
    return date.decode()

#Receives the message from a remote server
#@param socket - the socket of a remote server
#Returns - the message from a remote server as binary data
def receiveRemoteServerMessage(socket):
    message=b''
    line=socket.recv(1024)
    while line!=b'':
        message+=line
        line=socket.recv(1024)
    return message

#Receives the message sent from a client socket
#@param socket - a client socket
#Returns - the message send from the client socket
def receiveMessage(socket):
    message=''
    while '\r\n\r\n' not in message:
        message+=socket.recv(1024).decode()
    return message

#Parses the url of a client's get request
#Condition 1: If the url's path is a special absolute path, manipulates a global variable and returns a 
#Condition 2: Else extract the hostname, port, and path from the url and add them to a JSON. Also creates a get request message from the hostname, port, and path, to be added to the JSON.
#@param url - the url of a client's get request 
#Returns - a JSON containing a get request message, hostname, port, and path from the url if condition 1 or an  empty JSON if condition 2
def readUrl(url):
    #Removes the http:// portion of the url
    url=url[7:]
    urlParts=url.split('/')

    #Extracts the path of the url
    path='/'
    i=1
    while i<len(urlParts):
        path+=urlParts[i]
        if i<len(urlParts)-1:
            path+='/'
        i+=1

    requestJSON=[]

    #Checks the url's path and perform the necessary action depending on the path

    #Caching is enabled
    if path=='/proxy/cache/enable':
        enableCache()
    #Caching is disabled
    elif path=='/proxy/cache/disable':
        disableCache()
    #Domain blocking is enabled
    elif path=='/proxy/blocklist/enable':
        enableBlock()
    #Domain blocking is disabled
    elif path=='/proxy/blocklist/disable':
        disableBlock()
    #Empties list of cached objects
    elif path=='/proxy/cache/flush':
        emptyCacheList()
    #Empties list of blocked domains
    elif path=='/proxy/blocklist/flush':
        emptyBlockList()
    #Adds a domain to the list of blocked domains
    elif len(path)>21 and path[:21]=='/proxy/blocklist/add/':
        addToBlockList(path[21:])
    #Removes a domain from the list of blocked domains
    elif len(path)>24 and path[:24]=='/proxy/blocklist/remove/':
        removeFromBlockList(path[24:])
    #Extract the hostname, port, and path from the url to create a get request
    #Puts the hostname, port, path, and message to a JSON
    else:
        hostNPortJSON=urlParts[0].split(':')
        host=hostNPortJSON[0]
        port=80
        if len(hostNPortJSON)==2:
            port=int(hostNPortJSON[1])
        
        serverMessage='GET '+path+' HTTP/1.0'
        serverMessage+='\r\n'
        serverMessage+= 'Host: '+host
        serverMessage+='\r\n'
        serverMessage+='Connection: close'
        serverMessage+='\r\n'

        requestJSON.append(serverMessage)
        requestJSON.append(host)
        requestJSON.append(port)
        requestJSON.append(path)
    return requestJSON

#Parses the headers of a client's get request
#@param messageLines - the headers of a get request
#Returns - either a string error code if the headers aren't properly formatted or the headers as a string (except the Connection header)
def readHeaders(messageLines):
    headers=''
    i=1
    while i<len(messageLines):
        line=messageLines[i]
        lineParts=line.split(' ')
        if len(lineParts)<2:
            return '404'

        x=lineParts[0]
        if x[len(x)-1:]!=':':
            return '404'
        elif lineParts[0]!='Connection:':
            headers+=line+'\r\n'
        i+=1
    headers+='\r\n'
    return headers

#Given a JSON containing a message, hostname, and port, sends the message to a remote server at hostname and port
#@param requestJSON - a JSON containing a message, hostname, and port number
#Returns - the response from the remote server as binary data
def sendRequestToServer(requestJSON):
    clientSocket = socket(AF_INET, SOCK_STREAM)
    clientSocket.connect((requestJSON[1],requestJSON[2]))
    clientSocket.send(requestJSON[0].encode())
    serverResponse = receiveRemoteServerMessage(clientSocket)
    clientSocket.close()
    return serverResponse

#Given a int value, return the corresponding error message
#@param code - an int value corresponding to a error message
#Returns - a string error message
def error_message(code):
    if code==404:
        return 'HTTP/1.0 400 Bad Request\n'
    elif code == 505:
        return 'You must only support HTTP/1.0\n'
    elif code == 501:
        return 'HTTP/1.0 501 Not Implemented\n'
    elif code == 403:
        return 'HTTP/1.0 403 Forbidden\n'

#Checks if a client's get request is properly formatted and returns the appropriate message code
#200 means the message is correctly formatted or else, the message is incorrectly formatted
#@param request - a supposed get request from a client
#Returns - an int value correlating to a message code
def checkRequest(request):
    if len(request)!=3:
        return 404
    elif request[0]!='GET':
        return 501

    http=request[2].split('/')
    if len(http)!=2 or http[0]!='HTTP':
       return 404
    version=http[1]
    if version[:3]!='1.0':
        return 404

    url=request[1].split('/')
    if len(url)<4:
        return 404
    elif url[0]!= 'http:' or url[1]!= '':
        return 404

    return 200

#Reads a http get request from a client
#Sends back either an object or a error message to the client
#@param connectionSocket - socket to send and receive data from a client 
#@param addr - address bound to connectionSocket on the client's side
#Returns - void
def handleClient(connectionSocket, addr):
    #Receves a message from a client
    message= receiveMessage(connectionSocket)
    
    messageLines=message.split('\r\n')
    messageLines.pop()
    messageLines.pop()
    request=messageLines[0].split(' ')
    #Check if the get request line of the message is correctly formatted
    code=checkRequest(request)

    #If the request line is incrrectly formatted, send a error message to the client
    if code!=200:
        response=error_message(code)
        connectionSocket.send(response.encode())
    else:
        requestJSON=readUrl(request[1])
        #If the url in the client's get request has a specific absolute path, manipulate one of the global variables and send a 200 okay message to the client
        if len(requestJSON)==0:
            connectionSocket.send('HTTP/1.0 200 OK\n'.encode())
        else:
            headers=readHeaders(messageLines)
            #Checks if the headers in the client message are correctly formatted
            if headers=='404':
                response=error_message(404)
                connectionSocket.send(response.encode())
            else:
                host=requestJSON[1]
                port=requestJSON[2]
                path=requestJSON[3]
                #Blocks a connection to a domain if the client tries to connect to a blocked domain
                if blockEnable==True and inBlockList(host):
                    response=error_message(403)
                    connectionSocket.send(response.encode())
                #Sends a get request to a remote server if the object the client wants is not cached
                elif cacheEnable==False or (cacheEnable==True and host+path not in cacheList):
                    requestJSON[0]=requestJSON[0]+headers
                    response=sendRequestToServer(requestJSON)
                    connectionSocket.send(response)
                    date=getDate(response)
                    if cacheEnable==True and date!='':
                        responseJSON=[response,date]
                        addToCacheList(host+path,responseJSON)
                #The object the clients wants is cached
                elif cacheEnable==True and host+path in cacheList:
                    responseJSON=cacheList[host+path]
                    obj=responseJSON[0]
                    date=responseJSON[1]

                    #Makes a conditional get request to a remote server to see f the cached object is up to date
                    conditional='GET '+path+' HTTP/1.0'
                    conditional+='\r\n'
                    conditional+= 'Host: '+host
                    conditional+='\r\n'
                    conditional+= 'If-Modified-Since:'+date
                    conditional+='\r\n'
                    conditional+='Connection: close'
                    conditional+='\r\n\r\n'
                    conditionalJSON=[conditional, host, port]
                    condResponse=sendRequestToServer(conditionalJSON)
                    condResponseParts=condResponse.split(b'\r\n')
                    x=condResponseParts[0]
                    x=x[9:]
                    #Sends the cached object to the client if it's up to date
                    if x==b'304 Not Modified':
                        connectionSocket.send(obj)
                    #Sends a get request to a remote server to get a updated object
                    else:
                        requestJSON[0]=requestJSON[0]+headers
                        response=sendRequestToServer(requestJSON)
                        connectionSocket.send(response)
                        date=getDate(response)
                        if date!='':
                            responseJSON=[response,date]
                            addToCacheList(host+path,responseJSON)
                    
    #Closes connection to client
    connectionSocket.close()

    
# Start of program execution
# Parse out the command line server address and port number to listen to
parser = OptionParser()
parser.add_option('-p', type='int', dest='serverPort')
parser.add_option('-a', type='string', dest='serverAddress')
(options, args) = parser.parse_args()
port = options.serverPort
address = options.serverAddress
if address is None:
    address = 'localhost'
if port is None:
    port = 2100

# Set up signal handling (ctrl-c)
signal.signal(signal.SIGINT, ctrl_c_pressed)

# Set up sockets to receive requests
serverSocket.setsockopt(SOL_SOCKET, SO_REUSEADDR, 1)
serverSocket.bind((address,port))
serverSocket.listen()

while True:
    #Server accepts a client connection
    connectionSocket, addr = serverSocket.accept()
    #Server handles the client in a thread
    thread=threading.Thread(target=handleClient, args=(connectionSocket,addr))
    thread.start()

    
