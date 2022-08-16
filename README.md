# local-govt-reporter-scripts
This project is code written in C# .Net to scrape websites of Kansas City metro government agencies for upcoming meetings and store that meeting information in our local database.

The program runs in an EC2 instance.  The EC2 instance is started once per week, it launches the program, which collects the data, then the EC2 instance is shut down.
