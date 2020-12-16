package comprehensive;

import java.io.File;
import java.io.FileNotFoundException;
import java.util.ArrayList;
import java.util.List;
import java.util.Random;
import java.util.Scanner;

/**
 * Takes a grammar input file as parameter. Puts into the non-terminals and
 * production rules into a hash table, which combines separate chaining and
 * quadratic probing. Instead of a arraylist of using linked lists, uses a
 * arraylist of arraylists. Each different list within the arraylist holds a
 * nonterminal and its production rules. For example, one arraylist might hold
 * only nouns while another only hold verbs.
 * 
 * 
 * @author Dayi Hu & Alex Romero
 * @version April 18, 2019
 *
 */
public class hashArray {

	private ArrayList<ArrayList<String>> arr;
	private int size = 0;
	private int capacity = 0;
	private Random r = new Random();

	/**
	 * Creates a arraylist of arraylists and put the non-terminals and production
	 * rules from filename into a hash table.
	 * 
	 * @param - filename
	 */
	public hashArray(String filename) throws FileNotFoundException {
		arr = new ArrayList<ArrayList<String>>();

		for (int i = 0; i < 7; i++) {
			arr.add(new ArrayList<String>());
			capacity++;
		}

		buildHeapList(filename);
	}

	/**
	 * Builds a hash table with the non-terminals and production rules from
	 * filename.
	 * 
	 * @param - filename
	 * @throws - FileNotFoundException if filename doesn't exist or can't be
	 *         located.
	 */
	private void buildHeapList(String filename) throws FileNotFoundException {

		String next = "";
		String key = "";
		int hashCode = 0;
		File file = new File(filename);
		boolean readingTerminals = false;

		try (Scanner scan = new Scanner(file)) {

			while (scan.hasNextLine()) {

				next = scan.nextLine();

				if (next.equals("{")) {

					readingTerminals = true;

				} 

				else if (readingTerminals && next.contains("<")) {

					key = next;
					hashCode = hashFunction(key);

					while (!next.equals("}")) {
						arr.get(hashCode).add(next);
						next = scan.nextLine();
					}
					readingTerminals = false;
					size++;
					if (size == (capacity / 2)) {
						resize();
					}

				}
			}

		} catch (FileNotFoundException e) {
			e.printStackTrace();
		}
	}

	/**
	 * Doubles the capacity of the hash table backing hashArray if it's half full.
	 */
	private void resize() {

		capacity = capacity * 2;

		ArrayList<ArrayList<String>> newTable = new ArrayList<ArrayList<String>>();

		for (int i = 0; i < capacity; i++) {
			newTable.add(new ArrayList<String>());
		}

		ArrayList<ArrayList<String>> mappings = arr;

		arr = newTable;

		for (int i = 0; i < mappings.size(); i++) {
			if (mappings.get(i).size() != 0) {
				newTable.set(hashFunction(mappings.get(i).get(0)), mappings.get(i));
			}
		}
		arr = newTable;
	}

	/**
	 * Generates a Hash Function based on the hash-code from java, a prime number,
	 * and the table size. Uses quadratic probing if the list at the hash-code is
	 * already taken
	 * 
	 * @param s
	 * @return the (ideally) unique Hash
	 */
	private int hashFunction(String s) {
		int hash = Math.abs(s.hashCode() * 109) % arr.size();
		int i = 1;
		while (arr.get(hash).size() > 0) {
			hash = Math.abs(hash + (i * i)) % arr.size();
			i++;
		}
		return hash;
	}

	/**
	 * Finds and returns the hash code of String s.
	 * 
	 * @param s
	 * @return hash code
	 */
	private int findHashCode(String s) {

		int hash = Math.abs(s.hashCode() * 109) % arr.size();

		int i = 1;
		// !arr.get(hash).get(0).contains(s)
		while (!s.contains(arr.get(hash).get(0))) {
			hash = Math.abs(hash + (i * i)) % arr.size();
			i++;
		}

		return hash;
	}

	/**
	 * Returns the hash table backing hashArray.
	 * 
	 * @return the hash table backing hashArray
	 */
	public List<ArrayList<String>> list() {
		ArrayList<ArrayList<String>> temp = arr;
		return temp;
	}

	/**
	 * Prints out a number of random phrases based on howManyPhrases.
	 * 
	 * @param howManyPhrases
	 */
	public void generatePhrases(int howManyPhrases) {

		for (int i = 0; i < howManyPhrases; i++) {

			System.out.println(createRandomSentence("<start>"));
			// createRandomSentence("<start>");

		}

	}

	/**
	 * Looks at a string with no white spaces and converts and returns a string with
	 * all non-terminals converted to terminals.
	 * 
	 * @param something
	 * @return a string with all non-terminals converted to terminals
	 */
	public String createRandomWord(String something) {

		int start = 0;
		int end = 0;
		boolean enteredNonTermominal = false;
		String word = "";

		for (int i = 0; i < something.length(); i++) {
			if (something.charAt(i) == '<') {
				start = i;
				enteredNonTermominal = true;
			} else if (something.charAt(i) == '>') {

				end = i;

				String subString = something.substring(start, end + 1);
				ArrayList<String> stuff = arr.get(findHashCode(subString));
				int position = r.nextInt(stuff.size());

				if (position == 0) {
					position += 1;
				}
				String randomString = stuff.get(position);

				if (randomString.contains("<")) {
					randomString = createRandomSentence(randomString);
				}
				word += randomString;
				enteredNonTermominal = false;

			} else if (!enteredNonTermominal) {
				word += something.charAt(i);
			}

		}

		return word;
	}

	/**
	 * Converts and returns String input, typically with whitespaces, with all
	 * non-terminals converted to terminals.
	 * 
	 * @param input
	 * @return String input with all non-terminals converted to terminals
	 */
	public String createRandomSentence(String input) {

		String[] words = input.split(" ");
		StringBuilder sentence = new StringBuilder();

		for (int i = 0; i < words.length; i++) {
			String something = words[i];
			/*
			 * if the current word is a non-terminal i.e contains "<" find the placed that
			 * the non-terminal was hashed to and use that list to pick a random word
			 */
			if (something.contains("<")) {

				sentence.append(createRandomWord(something) + " ");

			} else {
				sentence.append(something + " ");
			}
		}

		return sentence.toString().trim();
	}
}
