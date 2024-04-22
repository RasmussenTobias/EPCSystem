import 'package:http/http.dart' as http;
import 'dart:convert';

class UserService {
  static Future<List<Map<String, dynamic>>> getUsers() async {
    final response = await http.get(Uri.parse('http://localhost:5256/Users'));
    if (response.statusCode == 200) {
      final List<dynamic> data = json.decode(response.body);
      return data.map((user) => {'id': user['id'], 'username': user['username']}).toList();
    } else {
      throw Exception('Failed to load users');
    }
  }
}