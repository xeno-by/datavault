@IF EXIST "%SUBWCREV_PATH%" (
    @ECHO Using SubWCRev from "%SUBWCREV_PATH%"
    "%SUBWCREV_PATH%" %1 %2 %3 %4
) ELSE (
    @IF EXIST "C:\Program Files\TortoiseSVN\bin\SubWCRev.exe" (
        @ECHO Using SubWCRev from "C:\Program Files\TortoiseSVN\bin\SubWCRev.exe"
        "C:\Program Files\TortoiseSVN\bin\SubWCRev.exe" %1 %2 %3 %4
    ) ELSE (
        @ECHO Cannot find SubWCRev.exe file. But fear not! It's not something weird or proprietary - it's freely distributed with TortoiseSVN. Please, install that, set the SUBWCREV_PATH environment variable to the full path to SubWCRev.exe and rerun this script.
        EXIT 1
    )        
)
