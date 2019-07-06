

import java.util.*;
import java.net.*;
import java.io.*;

//-----------------------------------------------
// File: Program2.java
// Author: Chris Kelley
// Date Created: 10/11/2018
// Last Modified: 10/18/2018
// Description: Takes input from user for a city and
// prints weather date for that city. 
//------------------------------------------------
//-----------------------------------
// Program2 class
// Prompts city name and country name from user and
// prints to the console weather data for that city.
public class Program2 {

    //main method
    public static void main(String[] args) {

        boolean goAgain = true;
        //loops while user indicates they want info for another city
        while (goAgain) {
            Scanner in = new Scanner(System.in);

            System.out.println("Enter city name (for city zip press 'z'): ");
            String city = in.nextLine().trim();

            boolean isZip = false;
            if (city.toUpperCase().equals("Z")) {
                System.out.println("Enter zip code: ");
                city = in.nextLine().trim();
                isZip = true;
            }

            if (isZip) {
                System.out.println("Enter country name or country code(required "
                        + "for zip codes not in the US): ");
            } else {
                System.out.println("Enter country name or country Code (press Enter if not using country): ");
            }
            String country = in.nextLine().trim();
            System.out.println("Press 'c' for current weather, 'f' for 5 day forecast");
            String type = in.nextLine().trim().toUpperCase();
            boolean isForecast = false;
            if(type.equals("F"))
            {
                 isForecast = true;
            }


            WeatherFetcher weatherFetcher = new WeatherFetcher(city, country);
            weatherFetcher.setIsZip(isZip);
            weatherFetcher.isForecast = isForecast;
            boolean success = weatherFetcher.fetchWeather();

            if (success) {
                if(!isForecast){
                weatherFetcher.printWeather();
                }else
                {
                    weatherFetcher.printForecast();
                }
            }

            System.out.println("Would you like to get the weather for another city? y/n");
            String response = in.nextLine().trim();
            if (response.toUpperCase().equals("Y")) {
                goAgain = true;
            } else {
                goAgain = false;
            }
        }
    }
}
