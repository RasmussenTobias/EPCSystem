import 'package:flutter/material.dart';
import 'device_service.dart'; // Import DeviceService here
import 'devicedto.dart';

class DevicePage extends StatefulWidget {
  final String username;
  final int userId;

  DevicePage({required this.userId, required this.username});

  @override
  _DevicePageState createState() => _DevicePageState();
}

class _DevicePageState extends State<DevicePage> {
  List<Map<String, dynamic>> _devices = [];
  TextEditingController _deviceNameController = TextEditingController();
  TextEditingController _locationController = TextEditingController();
  int _selectedRow = -1;

  @override
  void initState() {
    super.initState();
    _fetchDevices();
  }

  Future<void> _fetchDevices() async {
    try {
      List<Map<String, dynamic>> devices = await DeviceService.getDevicesByUsername(widget.username);
      setState(() {
        _devices = devices;
      });
    } catch (e) {
      // Handle error
      print('Failed to fetch devices: $e');
    }
  }

  List<Map<String, dynamic>> _searchDevices(String deviceNameQuery, String locationQuery) {
    return _devices.where((device) {
      final deviceName = device['deviceName'].toLowerCase();
      final location = device['location'].toLowerCase();

      return deviceName.contains(deviceNameQuery.toLowerCase()) && location.contains(locationQuery.toLowerCase());
    }).toList();
  }

  void _toggleRowSelection(int index) {
    setState(() {
      if (_selectedRow == index) {
        _selectedRow = -1;
      } else {
        _selectedRow = index;
      }
    });
  }

  void _showAddDeviceDialog() {
    showDialog(
      context: context,
      builder: (BuildContext context) {
        return AlertDialog(
          title: Text('Add Device'),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextField(
                controller: _deviceNameController,
                decoration: InputDecoration(labelText: 'Device Name'),
              ),
              TextField(
                controller: _locationController,
                decoration: InputDecoration(labelText: 'Location'),
              ),
            ],
          ),
          actions: [
            TextButton(
              onPressed: () {
                Navigator.of(context).pop();
              },
              child: Text('Cancel'),
            ),
            ElevatedButton(
              onPressed: () async {
                // Validate input and add device
                if (_deviceNameController.text.trim().isEmpty || _locationController.text.trim().isEmpty) {
                  ScaffoldMessenger.of(context).showSnackBar(
                    SnackBar(
                      content: Text('Please fill in all fields.'),
                      backgroundColor: Colors.red,
                    ),
                  );
                  return;
                }

                // Add device                
                try {
                  DeviceDto newDevice = DeviceDto(
                    userId: widget.userId, // Access user ID from widget
                    deviceName: _deviceNameController.text.trim(),
                    location: _locationController.text.trim(),
                  );

                  Map<String, dynamic> addedDevice = await DeviceService.addDevice(newDevice);
                  // Do something with addedDevice if needed

                  // Close the dialog
                  Navigator.of(context).pop();

                  // Show feedback to the user
                  ScaffoldMessenger.of(context).showSnackBar(
                    SnackBar(
                      content: Text('Device added successfully'),
                      backgroundColor: Colors.green,
                    ),
                  );

                  // Update the table to display the newly inserted device
                  setState(() {
                    _devices.add(addedDevice);
                  });

                  // Clear text fields
                  _deviceNameController.clear();
                  _locationController.clear();
                } catch (e) {
                  print('Error adding device: $e');
                  ScaffoldMessenger.of(context).showSnackBar(
                    SnackBar(
                      content: Text('Failed to add device. Please try again.'),
                      backgroundColor: Colors.red,
                    ),
                  );
                }
              },
              child: Text('Add'),
            ),
          ],
        );
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        automaticallyImplyLeading: false,
        title: Text('Devices'),
        actions: [
          IconButton(
            onPressed: _showAddDeviceDialog,
            icon: Icon(Icons.add),
          ),
        ],
      ),
      body: Column(
        children: [
          Padding(
            padding: const EdgeInsets.only(left:16.0,right: 16.0),
            child: Row(
              children: [
                Expanded(
                  child: SizedBox(
                    height: 40,
                    child: TextField(
                      controller: _deviceNameController,
                      onChanged: (value) {
                        setState(() {});
                      },
                      decoration: InputDecoration(
                        hintText: 'Search by Device Name',
                        border: OutlineInputBorder(),
                      ),
                    ),
                  ),
                ),
                SizedBox(width: 8),
                Expanded(
                  child: SizedBox(
                    height: 40,
                    child: TextField(
                      controller: _locationController,
                      onChanged: (value) {
                        setState(() {});
                      },
                      decoration: InputDecoration(
                        hintText: 'Search by Location',
                        border: OutlineInputBorder(),
                      ),
                    ),
                  ),
                ),
              ],
            ),
          ),
          Expanded(
            child: _devices.isEmpty
                ? Center(
                    child: CircularProgressIndicator(), // Show a loading indicator while fetching devices
                  )
                : SingleChildScrollView(
                    child: Container(
                      width: double.infinity, // Make the table go to the full width of the page
                      child: DataTable(
                        columns: [
                          DataColumn(label: Text('Device')),
                          DataColumn(label: Text('Location')),
                          DataColumn(label: Text('Total Production')),
                        ],
                        rows: _searchDevices(_deviceNameController.text, _locationController.text).map((device) {
                          return DataRow(
                            selected: device['selected'] ?? false,
                            onSelectChanged: (selected) {
                              setState(() {
                                device['selected'] = selected!;
                              });
                            },
                            cells: [
                              DataCell(Row(
                                children: [
                                  Icon(Icons.device_hub),
                                  SizedBox(width: 8),
                                  Text(device['deviceName']),
                                ],
                              )),
                              DataCell(Row(
                                children: [
                                  Icon(Icons.location_on),
                                  SizedBox(width: 8),
                                  Text(device['location']),
                                ],
                              )),
                              DataCell(Row(
                                children: [
                                  Icon(Icons.bar_chart), // Icon for total production value
                                  SizedBox(width: 4),
                                  Text(device['totalProduction'].toString()),
                                ],
                              )),
                            ],
                          );
                        }).toList(),
                      ),
                    ),
                  ),
          ),
        ],
      ),
    );
  }
}
