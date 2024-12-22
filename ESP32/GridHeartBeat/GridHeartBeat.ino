#include <WiFi.h>
#include <esp_spiffs.h>
#include <WiFiManager.h>
#include <WiFiUDP.h>
#include <ArduinoJson.h>

#define JSON_CONFIG_FILE "/spiffs/config.json"
#define DEVICE_NAME "DEVICE_NAME"

// UDP Configuration
WiFiUDP udp;
const int udpPort = 9123;
bool shouldSaveConfig = false;

// Variables to hold data from custom textboxes
char deviceName[50] = "device name";

// Define WiFiManager Object
WiFiManager wm;

void setup() {
    Serial.begin(115200);
    delay(10);

    Serial.println("Starting setup...");
    SpiffsSetup();
    WiFiSetup();
  
    if (shouldSaveConfig) {
        saveConfigFile();
    }
}
void broadcastMessage() {
  // Calculate the broadcast address for the connected network
  IPAddress localIP = WiFi.localIP();
  IPAddress subnetMask = WiFi.subnetMask();
  IPAddress broadcastAddress = (localIP | ~subnetMask);

  // Print the broadcast address for debugging
  Serial.printf("Broadcast Address: %s\n", broadcastAddress.toString().c_str());

  char message[100];
  snprintf(message, sizeof(message), "Heartbeat: %s", deviceName);

  // Broadcast the message
  udp.beginPacket(broadcastAddress, udpPort); // Start a packet to the calculated broadcast address
  udp.write((const uint8_t*)message, strlen(message)); // Write the message as bytes
  udp.endPacket();                            // Send the packet

  Serial.println(message);
}
void loop() {
  broadcastMessage();
  delay(2000);
}
void saveConfigFile() {
    Serial.println(F("Saving configuration..."));

    // Create a JSON document
    StaticJsonDocument<512> json;
    json[DEVICE_NAME] = deviceName;

    // Serialize JSON into a string
    String jsonString;
    serializeJson(json, jsonString);

    // Open config file for writing
    FILE *configFile = fopen(JSON_CONFIG_FILE, "w");
    if (!configFile) {
        Serial.println("Failed to open config file for writing");
        return;
    }

    // Write the serialized JSON string to the file
    size_t written = fwrite(jsonString.c_str(), sizeof(char), jsonString.length(), configFile);
    if (written != jsonString.length()) {
        Serial.println(F("Failed to write the entire JSON string to the file"));
    } else {
        Serial.println(F("Configuration saved successfully"));
    }

    // Close the file
    fclose(configFile);
}

bool loadConfigFile() {
    Serial.println("Loading configuration...");

    // Check if the configuration file exists
    FILE *configFile = fopen(JSON_CONFIG_FILE, "r");
    if (!configFile) {
        Serial.println("Configuration file not found");
        return false;
    }

    // Read the file into a buffer
    fseek(configFile, 0, SEEK_END);
    size_t fileSize = ftell(configFile);
    rewind(configFile);

    if (fileSize == 0) {
        Serial.println("Configuration file is empty");
        fclose(configFile);
        return false;
    }

    char *fileContent = (char *)malloc(fileSize + 1);
    if (!fileContent) {
        Serial.println("Failed to allocate memory for reading file");
        fclose(configFile);
        return false;
    }

    fread(fileContent, sizeof(char), fileSize, configFile);
    fileContent[fileSize] = '\0'; // Null-terminate the buffer

    fclose(configFile);

    // Deserialize the JSON
    StaticJsonDocument<512> json;
    DeserializationError error = deserializeJson(json, fileContent);
    free(fileContent);

    if (error) {
        Serial.println(F("Failed to parse JSON configuration file"));
        return false;
    }

    // Load values from the JSON file
    strcpy(deviceName, json[DEVICE_NAME]);
    Serial.println(F("Configuration loaded successfully"));
    return true;
}

void saveConfigCallback() {
    Serial.println("Configuration changes detected. Marking for save...");
    shouldSaveConfig = true;
}

void configModeCallback(WiFiManager *myWiFiManager) {
    Serial.println("Entered Configuration Mode");

    Serial.print("Config SSID: ");
    Serial.println(myWiFiManager->getConfigPortalSSID());

    Serial.print("Config IP Address: ");
    Serial.println(WiFi.softAPIP());
}

void SpiffsSetup()
{

    // Mount SPIFFS and format if necessary
    esp_vfs_spiffs_conf_t conf = {
        .base_path = "/spiffs",
        .partition_label = NULL,
        .max_files = 5,
        .format_if_mount_failed = true // Format if mount fails
    };

    esp_err_t ret = esp_vfs_spiffs_register(&conf);
    if (ret != ESP_OK) {
        Serial.printf("Failed to mount SPIFFS (%s)\n", esp_err_to_name(ret));
        return;
    }
}
void WiFiSetup()
{
      bool spiffsSetup = loadConfigFile();
    if (!spiffsSetup) {
        Serial.println("No saved configuration found. Forcing configuration mode...");
    }
   // Explicitly set WiFi mode
    WiFi.mode(WIFI_STA);

    // Set callback functions
    wm.setSaveConfigCallback(saveConfigCallback);
    wm.setAPCallback(configModeCallback);

    // Custom parameters
    WiFiManagerParameter custom_text_box("key_text", "Device Name", deviceName, sizeof(deviceName));
    wm.addParameter(&custom_text_box);

    // Configure WiFiManager behavior
    if (spiffsSetup) {
        if (!wm.autoConnect("PowerGrid")) {
            Serial.println("Failed to connect to WiFi. Restarting...");
            ESP.restart();
        }
    } else {
        if (!wm.startConfigPortal("PowerGrid_AP")) {
            Serial.println("Failed to start configuration portal. Restarting...");
            ESP.restart();
        }
    }

    // Connected to WiFi
    Serial.println("WiFi connected");
    Serial.printf("IP Address: %s\n", WiFi.localIP().toString().c_str());

    // Save custom parameters
    strncpy(deviceName, custom_text_box.getValue(), sizeof(deviceName));
    Serial.printf("Device Name: %s\n", deviceName);
 
}
