import 'dart:convert';
import 'package:http/http.dart' as http;

class TransferService {
  static Future<String> sendTransfer({
    required int fromUserId,
    required int toUserId,
    required List<Map<String, dynamic>> transfers, // Define the 'transfers' parameter
  }) async {
    try {
      var requestBody = {
        'fromUserId': fromUserId.toString(),
        'toUserId': toUserId.toString(),
        'transfers': transfers, // Use the provided transfers list
      };

      final response = await http.post(
        Uri.parse('http://localhost:5256/transfer'), // Replace with your backend URL
        headers: {'Content-Type': 'application/json'},
        body: json.encode(requestBody),
      );

      if (response.statusCode == 200) {
        return 'Transfer successful';
      } else {
        return 'Failed to send transfer: ${response.body}';
      }
    } catch (e) {
      return 'Failed to send transfer: $e';
    }
  }
}
