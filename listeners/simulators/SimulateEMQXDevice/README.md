

## Guide

1. Edit `appsettings.json` file, update `ConnectionString` field to your device's connection string.
2. Edit `appsettings.json` file, update `UsingMqtt` to publish message using mqtt or coap (true for mqtt, false for coap).
3. Edit `appsettings.json` file, update `SendMessage` to send or receive message (true for sending, false for receiving). Only apply for coap. If you using mqtt it will send/receive at the same time.
4. Edit `appsettings.json` file, update `DelayInMilliseconds` to set delay time.
5. Update method GetPayload (if needed) in SendMessage.cs to edit payload type.

