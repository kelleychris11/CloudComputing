


//----------------------
// File: ForecastData.java
// Date Created: 10/19/2018
// Author: Chris Kelley
// Purpose: Classes used to store information
// from JSON request for forecast weather data.
public class ForecastData {

    public String cod;
    public float message;
    public int cnt;
    public List[] list;
    public City city;
}

class List {

    public int dt;
    public Main main;
    public Weather[] weather;
    public Clouds clouds;
    public Wind wind;
    public Sys sys;
    public String dt_txt;
}

   class City
    {
        public int id;
        public String name;
        public String country;
        public int population;
    }
