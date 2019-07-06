import java.io.*;
import java.net.*;

//---------------------------------------------------------------
// File: Program1.java
// Author: Chris Kelley
// Date Created: 10/3/2018
// Last Modified: 10/10/2018
// Purpose: Implement WebCrawler object that retrieves html from
// a specified web page and finding the first absolute URL reference
// in the html. The program then gets the html for the found URL and
// repeats the process for a specified number of times.
//------------------------------------------------------------


//-----------------------------------------------
// Program1 class
// Purpose: initializes WebCrawler object instance.
//------------------------------------------------
public class Program1 {

    
	//Description: main method, initializes program
	//Parameter: args[0] is initial starting URL
	//Parameter: args[1] is number of URL hops program should attempt
    public static void main(String[] args) {       
		
		if(args.length < 2){
			System.out.println("Command line arguments are: URL numHops");
			return;
		}

		int hopsRequested;
		try{
        hopsRequested = Integer.parseInt(args[1]);
		}catch(NumberFormatException nfe){
			System.out.println("Command line arguments are: URL numHops");
			return;
		}
        String initialURL = args[0];

		//create WebCrawler, intitialize crawl
        WebCrawler crawler = new WebCrawler(initialURL, hopsRequested);
        crawler.startCrawl();
        crawler.printResults();
    }
}

//--------------------------------------------------------------
// WebCrawler class
// Purpose: Retrieves html from initial URL provided then finds
//  first absolute URL reference and gets html for that page. 
//  This process is repeated for the number of hops specified.
//  Crawl terminates when number of hops is reached, abnormal 
//  action occurs, error response code is encountered, no URLs
//  are found in the html, or number of specified hops is reached.
class WebCrawler {

    private String[] visitedURLs;
	private String[] redirectURLs;
	private int numRedirects;
    private int hopsRequested;
    private int currentHops = 0;
    private String nextURL;
    private String currPageHtml;
    private HttpURLConnection httpConn;
	private String terminationMessage;
	private URL url;

	// WebCrawler constructor
	// Parameter: initialURL is starting URL
	// Parameter: hopsRequested is number of hops to attempt
    public WebCrawler(String initialURL, int hopsRequested) {
        visitedURLs = new String[hopsRequested + 1];
		redirectURLs = new String[hopsRequested + 1];
        this.hopsRequested = hopsRequested + 1;
        this.nextURL = initialURL;
        currPageHtml = null;
        httpConn = null;
		numRedirects = 0;
		terminationMessage = null;
    }

	// Executes crawl. Searches html for URL absolute references
	// then gets html for found URLs and repeats process until
	// termination condition is encountered. 
    public void startCrawl() {
        while (currentHops <= hopsRequested) {

            if (!getURLConnection()) {
                return;
            }

            if (httpConn == null) {
                return;
            }

            if (!checkConnectionCode(httpConn)) {
                return;
            }
			
			String rawHtml;	
            try {
                rawHtml = getHtmlString(httpConn);
            } catch (IOException ioe) {
				terminationMessage = "Error reading html string";
                return;
            }

			currPageHtml = rawHtml;
            visitedURLs[currentHops] = nextURL;
            currentHops++;

            if (currentHops == hopsRequested) {
				//last hop done, no need to find next URL
                break;
            }

            String newURL = findNextURL();
            if (newURL == null) {
				terminationMessage = "No URLs found on current page";
                return;
            }
							
			if(newURL.lastIndexOf(".") > 0){
				String fileType = newURL.substring(newURL.lastIndexOf(".", newURL.length()));
				if(fileType.equals(".pdf") || fileType.equals(".jpg")
					|| fileType.equals(".png"))
					{
						terminationMessage = "Next URL found is " + fileType + " type";
						return;
					}
			}

            nextURL = newURL;
        }
    }

	//get connected with initialURL or URL found in html
	//return true if successfull
    private boolean getURLConnection() {
        try {
            url = new URL(nextURL);
        } catch (MalformedURLException mfurle) {
			terminationMessage = "Malformed URL encountered";
            return false;
        }

        try {
            httpConn = (HttpURLConnection) url.openConnection();
        } catch (IOException ioe) {
			terminationMessage = "IOException occurred: " + ioe;
            return false;
        }

        try {
            httpConn.setRequestMethod("GET");
        } catch (ProtocolException pe) {
			terminationMessage = "ProtocolException occurred: " + pe;
            return false;
        }

        return true;
    }

	//check response code of URL connection
	//return true if connection code is successful
    public boolean checkConnectionCode(HttpURLConnection httpConn) {
        int responseCode;
        try {
            responseCode = httpConn.getResponseCode();
        } catch (IOException ioe) {
			terminationMessage = "Error reading response code: " + ioe;
            return false;
        }

        if (responseCode >= 200 && responseCode < 300) {
            return true;
        }

		//check if redirect response code
        if (responseCode >= 300 && responseCode < 400) {

			//record URL before redirect to check for future duplicates
			redirectURLs[numRedirects] = nextURL;
			numRedirects++;
            nextURL = httpConn.getHeaderField("Location");

            if (!getURLConnection()) {
                return false;
            }

			try{
			responseCode = httpConn.getResponseCode();
			}catch(IOException ioe){
				terminationMessage = "Error reading response code: " + ioe; 
				return false;
			}
        }

        if (responseCode >= 400) {
			terminationMessage = "Error accessing URL: " 
			+ nextURL + "\nResponseCode: " + responseCode;
            return false;
        }
        return true;
    }

	//get html string from current URL connection.
    private String getHtmlString(HttpURLConnection httpConn) throws IOException {
        StringBuilder urlResponse = new StringBuilder();
        BufferedReader reader = new BufferedReader(new InputStreamReader(httpConn.getInputStream()));

        String input = reader.readLine();
        while (input != null) {
            urlResponse.append(input);
            input = reader.readLine();
        }

        reader.close();
        return urlResponse.toString();
    }

	//search html string for absolute URL references. Will not 
	//return URL that has already been visited.
	// returns URL String if found, if none found: returns null
    private String findNextURL() {
        String newURL = null;
        String rawHtml = currPageHtml;
        int htmlLength = rawHtml.length();
        int currIndex = 0;
        boolean urlFound = false;

        while ((currIndex + 6) <= htmlLength && !urlFound) {
			
			//check current 6 character long string to see if it matches "a href"
			// if no match, increment currIndex by one and check next string
            String currSlice = rawHtml.substring(currIndex, currIndex + 6);
            if (currSlice.equals("a href")) {
                int begIndexURL = currIndex + 8;
                int endIndexURL = begIndexURL + 1;

				//find endingIndex of URL string by searching for ending quotes ( " or ' )
                while ((!rawHtml.substring(endIndexURL, endIndexURL + 1).equals("\"")) 
				&& (!rawHtml.substring(endIndexURL, endIndexURL + 1).equals("\'"))) {
                    endIndexURL++;
                }

                newURL = rawHtml.substring(begIndexURL, endIndexURL);
                currIndex = endIndexURL;
                if (newURL.length() < 4) {
					if(newURL.startsWith("/")){
					newURL = url.getProtocol() + "://" + url.getHost() + newURL;
					}
					else{
						continue;
                    }
                }
                if (!newURL.substring(0, 4).equals("http")){
					if(newURL.startsWith("/")) {
					newURL = url.getProtocol() + "://" + url.getHost() + newURL;
					}
					else{
						continue;
					}
                }

				newURL = parseString(newURL);
				urlFound = !isDuplicateURL(newURL);
            } else {
                currIndex++;
            }
        }
        if(!urlFound){
          return null;  
        }
        return newURL;   
    }

	//gets rid of backslack at end of URL (if one exists)
    private String parseString(String foundURL) {

        if (foundURL.endsWith("/")) {
            return foundURL.substring(0, foundURL.length() - 1);
        }
        return foundURL;
    }

	//check to see if URL passed as parameter has already
	// been visited
	private boolean isDuplicateURL(String foundURL) {
		
		//check visited URLs
		for(int i = 0; i < currentHops; i++){
			if(visitedURLs[i].equals(foundURL)){
				return true;
			}
		}

		//check URLs that were redirects to URLs that
		//were visited
		for(int i = 0; i < numRedirects; i++)
		{
			if(redirectURLs[i].equals(foundURL))
			{
				return true;
			}
		}
		return false;
	}

	//print the results of the web crawl. 
	//prints html of last URL successfully visited
	//prints last URL successfully visited (html retrieved from)
	//prints termination message (if applicable) if crawl ended before
	//  number of hops requested
    public void printResults() {

        if (currentHops == 0) {
            System.out.println("No URL visited");
        } else {
            System.out.println(currPageHtml);
			System.out.println();
            System.out.println("Last URL visited: " + visitedURLs[currentHops - 1]);
			System.out.println();
            System.out.println("Number of Hops: " + (currentHops - 1));
        }
		
		if(terminationMessage != null)
		{
			System.out.println(terminationMessage);
		}
    }
}
