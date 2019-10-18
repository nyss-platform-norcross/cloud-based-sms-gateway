# cloud-based-sms-gateway

## Configuration

To configure the Application, you will need to put the Twilio Sid and the twilio token wich you can find in your settings there:
![Twilio settings panel with Live Credentials](https://github.com/nyss-platform-norcross/cloud-based-sms-gateway/blob/pictures/pictures/twilio.png)
After this, you will need to go inside the local.settings.json of the function app and put inside the credentials like this:
![Local settings json with credentials](https://github.com/nyss-platform-norcross/cloud-based-sms-gateway/blob/pictures/pictures/localsettings.png)

## Use it

After it, you can publish on Azure and you will only need to put the adress give it by Azure with '/api/Twilio' inside the settings of the phone number like this:
![Twilio Phone Number settings](https://github.com/nyss-platform-norcross/cloud-based-sms-gateway/blob/pictures/pictures/twiliophonenumber.png)
