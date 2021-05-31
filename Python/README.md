# QuantForce

## Overview
This project is a QuantForce API implemtentation in Python

Have a look at the Wiki for more info

## Structure

- utils

    * The source code for the management of folders and files of this project
    
- output

    * Downloaded files from QuantForce API
    
- app

    The main execution file
    
- requirements

    All the dependencies for this project
    
- settings

    The several settings including file path
    
## Installation

- Environment

    Ubuntu 18.04, Windows 10, Python 3.6

- Dependency Installation

    Please navigate to the project directory and run the following command in the terminal
    ```
        pip3 install -r requirements.txt
    ``` 

## Execution

- Please set DATA_FILE_PATH in settings with the absolute path of csv data file to upload to api.

- Please run the following command in the terminal.

    ```
        python3 app.py -u user email -p password -pn projectname
    ```

- The downloaded files are saved in the output directory.
