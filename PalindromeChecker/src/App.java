import java.util.Scanner;

public class App {
    public static void main(String[] args) throws Exception {
        Scanner scanner = new Scanner(System.in);

        System.out.print("Input kata Palindrom : ");
        String inputVal = scanner.nextLine().trim();
        
        String reverseValue = new StringBuffer(inputVal).reverse().toString();

        if (inputVal.isBlank()) {
            System.out.println("Input is empty!!");
            return;
        }

        if (inputVal.equalsIgnoreCase(reverseValue)) {
            System.out.println(inputVal + " is a valid Palindrome!");
        } else {
            System.out.println(inputVal + " is not a valid Palindrome!");
        }
    }
}
