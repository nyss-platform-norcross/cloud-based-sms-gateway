# cloud-based-sms-gateway

## Configuration

To configure the Application, you will need to put the Twilio Sid and the twilio token wich you can find in your settings there:
![Twilio settings panel with Live Credentials](https://github.com/nyss-platform-norcross/cloud-based-sms-gateway/blob/develop/pictures/Twilio.png)
After this, you will need to go inside the local.settings.json of the function app and put inside the credentials like this:
![Local settings json with credentials](https://github.com/nyss-platform-norcross/cloud-based-sms-gateway/blob/develop/pictures/localsettings.png)
You will need to put the API url too by adding in the local.settings.json a value for it:
![Local settings json with Api Url](https://github.com/nyss-platform-norcross/cloud-based-sms-gateway/blob/develop/pictures/apiUrl.PNG)

## Use it

After it, you can publish on Azure and you will only need to put the adress give it by Azure with '/api/Twilio' inside the settings of the phone number like this:
![Twilio Phone Number settings](https://github.com/nyss-platform-norcross/cloud-based-sms-gateway/blob/develop/pictures/twiliophonenumber.png)
