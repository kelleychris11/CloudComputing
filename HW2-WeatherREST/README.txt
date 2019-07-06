Program has dependencies: gson-2.8.0.jar (path), gson-2.8.0-javadoc.jar
and gson-2.8.0-sources.jar

Program may be executed with the jar files from the command line:

java -jar Program2.jar


Jar file may be created with the the command:

jar cvfm Program2.jar manifest.txt *.class


Program may be compiled with .java files with (with gson files in same 
directory as other program files):

javac -cp .:gson-2.8.0.jar Program2.java WeatherFetcher.java WeatherData.java
ForecastData.java


Program may be run with compiled files with:

java -cp .:gson-2.8.0.jar Program2