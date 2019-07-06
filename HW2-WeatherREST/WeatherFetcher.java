

import com.google.gson.Gson;
import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.ProtocolException;
import java.net.URL;
import java.util.GregorianCalendar;

//---------------------------------------------------
// File: WeatherFetcher.java
// Author: Chris Kelley
// Date Created: 10/11/2018
// Last Modified: 10/18/2018
// Purpose: Consumes Restful service to obtain weather
// for a given city. Converts JSON data returned to Java
// objects. Prints weather data.
//----------------------------------------------------
//Consumes RESTful api to obtain, convert and print weather
// data for a city.
public class WeatherFetcher {

    private String apiKey = "1f62d6039a2f6883745abe62c3a8d5a6";
    private static String[][] countryCodes;
    private String countryCode;
    private String city;
    private String country;
    private boolean isZip;
    private WeatherData currWeather;
    private ForecastData forecast;
    private HttpURLConnection conn;
    boolean isForecast;

    //constructor
    public WeatherFetcher(String city, String country) {
        isZip = false;
        isForecast = false;
        this.city = city;
        this.country = country;

        //load country codes into an array
        countryCodes = getCountryCodes();

        //check if user entered a country
        if (!country.equals("")) {
            countryCode = findCountryCode(country);
            if (countryCode == null) {
                System.out.println("Country: " + country + " not found");
            }
        } else {
            countryCode = null;
        }
    }

    //set if city has zip code
    public void setIsZip(boolean isZip) {
        this.isZip = isZip;
    }

    //To calculate amount of wait time between
    //http connection requests
    public void waitTime(int attemptNum) {
        int millisecSleep = 0;
        if (attemptNum < 3) {
            millisecSleep = 100;
        } else if (attemptNum < 7) {
            millisecSleep = 500;
        } else if (attemptNum < 10) {
            millisecSleep = 1000;
        }
        try {
            Thread.sleep(millisecSleep);
        } catch (InterruptedException ie) {
            Thread.currentThread().interrupt();
        }
    }

    //Obtains weather data in JSON. Stores data in Java objects
    public boolean fetchWeather() {
        URL url = getURL();
        if (url == null) {
            return false;
        }

        int responseCode = 0;
        boolean success = true;
        //make repeat http calls in case connection error occurs
        for (int i = 0; i < 10; i++) {
            success = true;
            if (i > 0) {
                waitTime(i);
            }

            try {
                conn = (HttpURLConnection) url.openConnection();
            } catch (IOException ioe) {
                success = false;
                continue;
            }

            try {
                conn.setRequestMethod("GET");
                conn.addRequestProperty("x-api-key", apiKey);
            } catch (ProtocolException pe) {
                success = false;
                continue;
            }

            try {
                responseCode = conn.getResponseCode();
            } catch (IOException ioe) {
                success = false;
                continue;
            }
            if (responseCode != 200) {
                success = false;
                continue;
            }
            if (success == true) {
                break;
            }
        }

        if (responseCode != 200) {
            System.out.println("City not found");
            return false;
        }
        if (success = false) {
            System.out.println("Error obtaining weather data");
            return false;
        }

        String jsonResponse = null;
        try {
            jsonResponse = getJsonString();
        } catch (IOException ioe) {
            System.out.println("Error reading input");
        }

        Gson gson = new Gson();
        if (!isForecast) {
            currWeather = gson.fromJson(jsonResponse.toString(), WeatherData.class);
        } else {
            forecast = gson.fromJson(jsonResponse.toString(), ForecastData.class);
        }
        return true;
    }

    //get string of JSON data from http response
    public String getJsonString() throws IOException {

        StringBuilder jsonResponse = new StringBuilder();
        BufferedReader reader = new BufferedReader(new InputStreamReader(conn.getInputStream()));

        String input = reader.readLine();
        while (input != null) {
            jsonResponse.append(input);
            input = reader.readLine();
        }
        reader.close();
        return jsonResponse.toString();
    }

    //assemble and return URL for api call
    public URL getURL() {
        URL url = null;
        String path;
        if (countryCode != null) {
            if (!isZip) {
                path = "q=" + city + "," + countryCode + "&AAPID" + apiKey;
            } else {
                path = "zip=" + city + "," + countryCode + "&AAPID" + apiKey;
            }
        } else if (!isZip) {
            path = "q=" + city + "&AAPID" + apiKey;
        } else {

            path = "zip=" + city + ",US&AAPID" + apiKey;
        }

        if (!isForecast) {
            try {
                url = new URL("http://api.openweathermap.org/data/2.5/weather?" + path);
            } catch (MalformedURLException e) {
                System.out.println("malformed url");
                return null;
            }
        } else {
            try {
                url = new URL("http://api.openweathermap.org/data/2.5/forecast?" + path);
            } catch (MalformedURLException e) {
                System.out.println("malformed url");
                return null;
            }

        }
        return url;
    }
 
   //Print weather for city. 
    public void printForecast() {

        System.out.println("-------Weather Forecast For "
                + forecast.city.name + "," + forecast.city.country
                + "----------");
        GregorianCalendar date = new GregorianCalendar();
        System.out.println(date.getTime() + "\n");
        double avgCalc = 0;
        double highCalc = 0;
        double lowCalc = 2000;
        for (int i = 0; i < forecast.list.length; i++) {
            
            //calculate average, high, and low temperatures for the day
            avgCalc += forecast.list[i].main.temp;
            if(highCalc < forecast.list[i].main.temp_max){
                highCalc = forecast.list[i].main.temp_max;
            }
            
            if(lowCalc > forecast.list[i].main.temp_min)
            {
                lowCalc = forecast.list[i].main.temp_min;
            }
            
            if ((((i + 1) % 8) == 0) && i != 0) {
                avgCalc /= 8;
                int day = (i + 1) / 8;
                System.out.println(day + " day forecast: ");
                double avgTemp = ((avgCalc - 273.15) * 9/5) + 32;
                double lowTemp = ((lowCalc - 273.15) * 9/5) + 32;
                double highTemp = ((highCalc - 273.15) * 9/5) + 32;
                
                System.out.printf("Average Temp: %.2f F", avgTemp);
                System.out.printf(" (low: %.2f F -- high: %.2f F)\n", lowTemp, highTemp);
                System.out.println("Humidity: " + forecast.list[i].main.humidity + "%");
                for (int j = 0; j < forecast.list[i].weather.length; j++) {
                    if (j != 0) {
                        System.out.print(" & ");
                    }
                    System.out.print("Conditions: ");
                    System.out.print(forecast.list[i].weather[j].description);
                }
                System.out.println();
                System.out.println("");
                avgCalc = 0;
                highCalc = 0;
                lowCalc = 2000;
            }
        }
    }

    //print weather
    public void printWeather() {
        double currTempFahren = ((currWeather.main.temp - 273.15) * 9 / 5) + 32;
        double lowTempFahren = ((currWeather.main.temp_min - 273.15) * 9 / 5) + 32;
        double highTempFahren = ((currWeather.main.temp_max - 273.15) * 9 / 5) + 32;

        GregorianCalendar date = new GregorianCalendar();

        System.out.println("------Weather Report for " + currWeather.name + "," + currWeather.sys.country
                + "--------------");
        System.out.println(date.getTime() + "\n");
        System.out.printf("Current Temperature: %.2f Fahrenheit", currTempFahren);
        System.out.print(" (may be in range: ");
        System.out.printf("%.2f to %.2f F)\n\n", lowTempFahren, highTempFahren);

        System.out.print("Current Conditions: ");

        for (int i = 0; i < currWeather.weather.size(); i++) {
            if (i != 0) {
                System.out.print(" and ");
            }

            System.out.print(currWeather.weather.get(i).description);
        }
        System.out.println("\n");

        System.out.println("Current Humidity: " + currWeather.main.humidity + "%");
        System.out.println("Current Cloud Coverage: " + currWeather.clouds.all + "%");
        System.out.println("Current Wind Speed: " + currWeather.wind.speed + " meters/second");
        System.out.println("Current Wind Direction: " + currWeather.wind.deg + " degrees\n");
    }

    //return country code given a country name
    public String findCountryCode(String country) {

        for (int i = 0; i < countryCodes[0].length; i++) {
            if (country.toUpperCase().equals(countryCodes[0][i])
                    || country.toUpperCase().equals(countryCodes[1][i])) {
                return countryCodes[0][i];
            }
        }
        return null;
    }

    //print list of country codes
    public static void printCountryCodes() {

        for (int i = 0; i < countryCodes[0].length; i++) {
            System.out.println(countryCodes[0][i] + " \t" + countryCodes[1][i]);
        }
    }

    //returns country codes and country names in array
    public static String[][] getCountryCodes() {
        String countryString = "AF	,	Afghanistan\n"
                + "AX	,	Aland Islands\n"
                + "AL	,	Albania\n"
                + "DZ	,	Algeria\n"
                + "AS	,	American Samoa\n"
                + "AD	,	Andorra\n"
                + "AO	,	Angola\n"
                + "AI	,	Anguilla\n"
                + "AQ	,	Antarctica\n"
                + "AG	,	Antigua and Barbuda\n"
                + "AR	,	Argentina\n"
                + "AM	,	Armenia\n"
                + "AW	,	Aruba\n"
                + "AU	,	Australia\n"
                + "AT	,	Austria\n"
                + "AZ	,	Azerbaijan\n"
                + "BS	,	Bahamas\n"
                + "BH	,	Bahrain\n"
                + "BD	,	Bangladesh\n"
                + "BB	,	Barbados\n"
                + "BY	,	Belarus\n"
                + "BE	,	Belgium\n"
                + "BZ	,	Belize\n"
                + "BJ	,	Benin\n"
                + "BM	,	Bermuda\n"
                + "BT	,	Bhutan\n"
                + "BO	,	Bolivia\n"
                + "BA	,	Bosnia and Herzegovina\n"
                + "BW	,	Botswana\n"
                + "BV	,	Bouvet Island\n"
                + "BR	,	Brazil\n"
                + "IO	,	British Indian Ocean Territory\n"
                + "BN	,	Brunei Darussalam\n"
                + "BG	,	Bulgaria\n"
                + "BF	,	Burkina Faso\n"
                + "BI	,	Burundi\n"
                + "KH	,	Cambodia\n"
                + "CM	,	Cameroon\n"
                + "CA	,	Canada\n"
                + "CV	,	Cape Verde\n"
                + "KY	,	Cayman Islands\n"
                + "CF	,	Central African Republic\n"
                + "TD	,	Chad\n"
                + "CL	,	Chile\n"
                + "CN	,	China\n"
                + "CX	,	Christmas Island\n"
                + "CC	,	Cocos (Keeling) Islands\n"
                + "CO	,	Colombia\n"
                + "KM	,	Comoros\n"
                + "CG	,	Congo\n"
                + "CD	,	Congo, The Democratic Republic of \n"
                + "CK	,	Cook Islands\n"
                + "CR	,	Costa Rica\n"
                + "CI	,	Cote d'Ivoire\n"
                + "HR	,	Croatia\n"
                + "CU	,	Cuba\n"
                + "CY	,	Cyprus\n"
                + "CZ	,	Czech Republic\n"
                + "DK	,	Denmark\n"
                + "DJ	,	Djibouti\n"
                + "DM	,	Dominica\n"
                + "DO	,	Dominican Republic\n"
                + "TP	,	East Timor\n"
                + "EC	,	Ecuador\n"
                + "EG	,	Egypt\n"
                + "SV	,	El Salvador\n"
                + "GQ	,	Equatorial Guinea\n"
                + "ER	,	Eritrea\n"
                + "EE	,	Estonia\n"
                + "ET	,	Ethiopia\n"
                + "FK	,	Falkland Islands (Malvinas)\n"
                + "FO	,	Faroe Islands\n"
                + "FJ	,	Fiji\n"
                + "FI	,	Finland\n"
                + "FR	,	France\n"
                + "GF	,	French Guiana\n"
                + "PF	,	French Polynesia\n"
                + "TF	,	French Southern Territories\n"
                + "GA	,	Gabon\n"
                + "GM	,	Gambia\n"
                + "GE	,	Georgia\n"
                + "DE	,	Germany\n"
                + "GH	,	Ghana\n"
                + "GI	,	Gibraltar\n"
                + "GR	,	Greece\n"
                + "GL	,	Greenland\n"
                + "GD	,	Grenada\n"
                + "GP	,	Guadeloupe\n"
                + "GU	,	Guam\n"
                + "GT	,	Guatemala\n"
                + "GN	,	Guinea\n"
                + "GW	,	Guinea-Bissau\n"
                + "GY	,	Guyana\n"
                + "HT	,	Haiti\n"
                + "HM	,	Heard and Mc Donald Islands\n"
                + "VA	,	Holy See (Vatican City State)\n"
                + "HN	,	Honduras\n"
                + "HK	,	Hong Kong\n"
                + "HU	,	Hungary\n"
                + "IS	,	Iceland\n"
                + "IN	,	India\n"
                + "ID	,	Indonesia\n"
                + "IR	,	Iran, Islamic Republic of\n"
                + "IQ	,	Iraq\n"
                + "IE	,	Ireland\n"
                + "IL	,	Israel\n"
                + "IT	,	Italy\n"
                + "JM	,	Jamaica\n"
                + "JP	,	Japan\n"
                + "JO	,	Jordan\n"
                + "KZ	,	Kazakstan\n"
                + "KE	,	Kenya\n"
                + "KI	,	Kiribati\n"
                + "KP	,	Korea, Democratic People's Republic of\n"
                + "KR	,	Korea, Republic of\n"
                + "KW	,	Kuwait\n"
                + "KG	,	Kyrgyzstan\n"
                + "LA	,	Lao, People's Democratic Republic\n"
                + "LV	,	Latvia\n"
                + "LB	,	Lebanon\n"
                + "LS	,	Lesotho\n"
                + "LR	,	Liberia\n"
                + "LY	,	Libyan Arab Jamahiriya\n"
                + "LI	,	Liechtenstein\n"
                + "LT	,	Lithuania\n"
                + "LU	,	Luxembourg\n"
                + "MO	,	Macao\n"
                + "MK	,	Macedonia, The Former Yugoslav Republic Of\n"
                + "MG	,	Madagascar\n"
                + "MW	,	Malawi\n"
                + "MY	,	Malaysia\n"
                + "MV	,	Maldives\n"
                + "ML	,	Mali\n"
                + "MT	,	Malta\n"
                + "MH	,	Marshall Islands\n"
                + "MQ	,	Martinique\n"
                + "MR	,	Mauritania\n"
                + "MU	,	Mauritius\n"
                + "YT	,	Mayotte\n"
                + "MX	,	Mexico\n"
                + "FM	,	Micronesia, Federated States of\n"
                + "MD	,	Moldova, Republic of\n"
                + "MC	,	Monaco\n"
                + "MN	,	Mongolia\n"
                + "MS	,	Montserrat\n"
                + "MA	,	Morocco\n"
                + "MZ	,	Mozambique\n"
                + "MM	,	Myanmar\n"
                + "NA	,	Namibia\n"
                + "NR	,	Nauru\n"
                + "NP	,	Nepal\n"
                + "NL	,	Netherlands\n"
                + "AN	,	Netherlands Antilles\n"
                + "NC	,	New Caledonia\n"
                + "NZ	,	New Zealand\n"
                + "NI	,	Nicaragua\n"
                + "NE	,	Niger\n"
                + "NG	,	Nigeria\n"
                + "NU	,	Niue\n"
                + "NF	,	Norfolk Island\n"
                + "MP	,	Northern Mariana Islands\n"
                + "NO	,	Norway\n"
                + "OM	,	Oman\n"
                + "PK	,	Pakistan\n"
                + "PW	,	Palau\n"
                + "PA	,	Panama\n"
                + "PG	,	Papua New Guinea\n"
                + "PY	,	Paraguay\n"
                + "PE	,	Peru\n"
                + "PH	,	Philippines\n"
                + "PN	,	Pitcairn\n"
                + "PL	,	Poland\n"
                + "PT	,	Portugal\n"
                + "PR	,	Puerto Rico\n"
                + "QA	,	Qatar\n"
                + "RE	,	Reunion\n"
                + "RO	,	Romania\n"
                + "RU	,	Russia Federation\n"
                + "RW	,	Rwanda\n"
                + "SH	,	Saint Helena\n"
                + "KN	,	Saint Kitts & Nevis\n"
                + "LC	,	Saint Lucia\n"
                + "PM	,	Saint Pierre and Miquelon\n"
                + "VC	,	Saint Vincent and the Grenadines\n"
                + "WS	,	Samoa\n"
                + "SM	,	San Marino\n"
                + "ST	,	Sao Tome and Principe\n"
                + "SA	,	Saudi Arabia\n"
                + "SN	,	Senegal\n"
                + "CS	,	Serbia and Montenegro\n"
                + "SC	,	Seychelles\n"
                + "SL	,	Sierra Leone\n"
                + "SG	,	Singapore\n"
                + "SK	,	Slovakia\n"
                + "SI	,	Slovenia\n"
                + "SB	,	Solomon Islands\n"
                + "SO	,	Somalia\n"
                + "ZA	,	South Africa\n"
                + "GS	,	South Georgia & The South Sandwich Islands\n"
                + "ES	,	Spain\n"
                + "LK	,	Sri Lanka\n"
                + "SD	,	Sudan\n"
                + "SR	,	Suriname\n"
                + "SJ	,	Svalbard and Jan Mayen\n"
                + "SZ	,	Swaziland\n"
                + "SE	,	Sweden\n"
                + "CH	,	Switzerland\n"
                + "SY	,	Syrian Arab Republic\n"
                + "TW	,	Taiwan, Province of China\n"
                + "TJ	,	Tajikistan\n"
                + "TZ	,	Tanzania, United Republic of\n"
                + "TH	,	Thailand\n"
                + "TG	,	Togo\n"
                + "TK	,	Tokelau\n"
                + "TO	,	Tonga\n"
                + "TT	,	Trinidad and Tobago\n"
                + "TN	,	Tunisia\n"
                + "TR	,	Turkey\n"
                + "TM	,	Turkmenistan\n"
                + "TC	,	Turks and Caicos Islands\n"
                + "TV	,	Tuvalu\n"
                + "UG	,	Uganda\n"
                + "UA	,	Ukraine\n"
                + "AE	,	United Arab Emirates\n"
                + "GB	,	United Kingdom\n"
                + "US	,	United States\n"
                + "UM	,	United States Minor Outlying Islands\n"
                + "UY	,	Uruguay\n"
                + "UZ	,	Uzbekistan\n"
                + "VU	,	Vanuatu\n"
                + "VE	,	Venezuela\n"
                + "VN	,	Vietnam\n"
                + "VG	,	Virgin Islands, British\n"
                + "VI	,	Virgin Islands, U.S.\n"
                + "WF	,	Wallis and Futuna\n"
                + "EH	,	Western Sahara\n"
                + "YE	,	Yemen\n"
                + "ZM	,	Zambia\n"
                + "ZW	,	Zimbabwe\n";

        //parse country code string into array
        int currIndex = 0;
        String[][] codes = new String[2][239];
        for (int i = 0; i < codes[1].length; i++) {
            codes[0][i] = countryString.substring(currIndex, currIndex + 2);

            codes[1][i] = countryString.substring(countryString.indexOf(",", currIndex) + 2,
                    countryString.indexOf("\n", currIndex)).toUpperCase();
            currIndex = countryString.indexOf("\n", currIndex) + 1;

        }
        return codes;
    }
}
