package comprehensive;

import java.io.File;
import java.io.FileNotFoundException;
import java.util.ArrayList;
import java.util.List;

/**
 * Generate a number of random phrases by running the hashArray class with command line arguments.
 * The arguments are a input grammar file and number of phrases togenerate.
 * 
 * @author Dayi Hu & Alex Romero
 * @version April 18, 2019
 *
 */

public class RandomPhraseGenerator {

	public static void main(String[] args) {

		String filename = args[0];
		// String filename = "abc_spaces.g";
		// String filename = "super_simple.g";
		// String filename = "poetic_sentence.g";
		//String filename = "abc.g";
		//String filename = "mathematical_expression.g";
		//String filename = "assignment_extension_request.g";
		hashArray test;

		try {
			test = new hashArray(filename);
			test.generatePhrases(Integer.parseInt(args[1]));
			// test.generatePhrases(5);

		} catch (FileNotFoundException e) {
			e.printStackTrace();
		}

	}

}