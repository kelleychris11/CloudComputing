

import java.util.List;

//-------------------------------------------
// File: WeatherData.java
// Author: Chris Kelley
// Date Created: 10/11/2018
// Last Modified: 10/18/2018
// Purpose: Hold weather data converted from
// JSON, retrieved from weather REST api
//--------------------------------------------

public class WeatherData {

    Coord coord;
    double temp;
    Main main;
    Clouds clouds;
    Wind wind;
    List<Weather> weather;
    Sys sys;
    String name;
}

class Coord {
    //city coordinates
    double lon;
    double lat;
}

class Weather {

    int id;
    //catagory of weather conditions
    String main;
    //description of weather conditions
    String description;
}

class Main {

    double temp;
    double pressure;
    double humidity;
    //current possible min temp
    double temp_min;
    //current possible max temp
    double temp_max;
}

class Clouds {
    //clound coverage %
    double all;
}

class Wind {

    double speed;
    double deg;
}

class Sys
{
    String country;
    long sunrise;
    long sunset;
}




