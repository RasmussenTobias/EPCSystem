import 'dart:convert';
import 'package:http/http.dart' as http;

class CertificateService {
  static Future<List<Map<String, dynamic>>?> getUserCertificates(int userId) async {
    try {
      final response = await http.get(
        Uri.parse('http://localhost:5256/Certificates/user/$userId'),
        headers: {'accept': 'application/json'},
      );

      if (response.statusCode == 200) {
        final List<dynamic> data = json.decode(response.body);
        final List<Map<String, dynamic>> certificates = data.cast<Map<String, dynamic>>();
        return certificates;
      } else {
        // If the server did not return a 200 OK response,
        // then throw an exception.
        throw Exception('Failed to load certificates');
      }
    } catch (e) {
      // Handle any errors that occurred during the request.
      print('Error: $e');
      return null;
    }
  }
}
