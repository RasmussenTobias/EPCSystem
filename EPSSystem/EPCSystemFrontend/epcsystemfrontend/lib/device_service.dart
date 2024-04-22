import 'package:http/http.dart' as http;
import 'dart:convert';
import 'devicedto.dart';

class DeviceService {
  static Future<List<Map<String, dynamic>>> getDevicesByUsername(String username) async {
    final response = await http.get(Uri.parse('http://localhost:5256/devices/$username'));
    if (response.statusCode == 200) {
      List<dynamic> data = json.decode(response.body);
      return List<Map<String, dynamic>>.from(data);
    } else {
      throw Exception('Failed to fetch devices: ${response.statusCode}');
    }
  }

  static Future<Map<String, dynamic>> addDevice(DeviceDto device) async {
    final response = await http.post(
      Uri.parse('http://localhost:5256/Devices'),
      headers: <String, String>{
        'Content-Type': 'application/json; charset=UTF-8',
      },
      body: jsonEncode(device.toJson()),
    );

    if (response.statusCode == 201) {
      return jsonDecode(response.body);
    } else {
      throw Exception('Failed to add device: ${response.statusCode}');
    }
  }
}
