# VR Gadget remote Controller
An application that sends commands from a VR device to an edge device via MQTT to control hardware.

## Features

- **MQTT Communication**: Uses Beebotte MQTT broker for reliable command transmission
- **VR Gadget Control**: Simple control interface for VR devices including:
  - **Control Features**: Haptic feedback features to enhance the immersive experience in VR:
    - Heating control with a Peltier module (start/finish heating)
    - Cooling control with a Peltier module (start/finish cooling)
    - Splash control with a ultrasonic mist generator module (start/finish splash)
  - :information-sourceß: Usage:
    - You can combine heating/cooling and splash.  
    e.g. if you call `StartHeatingAsync()` and `StartSplashAsync()`, the gadget will heat and splash at the same time.
    - You cannot use heating and cooling at the same time.  
    e.g. if you call `StartHeatingAsync()` and later, `StartCoolingAsync()`, the gadget will cancel heating and start cooling.

## Project Structure

```
vr-gadget-controller/
├── VRGadgetController/
│   ├── VRGadgetController.csproj    # Project file with dependencies
│   ├── Program.cs                   # Main application entry point
│   └── Services/
│       ├── IVRGadgetController.cs   # Interface definition
│       └── VRGadgetController.cs    # MQTT-based implementation
└── README.md                        # This file
```

## Dependencies

- **.NET 9.0**: Target framework (not required in Unity environment)
- **MQTTnet 4.0.2.221**: MQTT client library
- **MQTTnet.Extensions.ManagedClient 4.0.2.221**: Extended MQTT client features

:warning: For Unity environment, please refer to the following link for setup:
https://github.com/makaizo/tesla-api-handler/blob/main/TeslaAPIHandler/Frontend/how_to_setup_unity.md

## Configuration

No configuration is required. The Beebotte token is hardcoded in the `VRGadgetController.cs` file to avoid Unity's complex file system setup. Also, it costs nothing to use Beebotte because it has a free tier.