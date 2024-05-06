import 'dart:convert';
import 'package:http/http.dart' as http;
import 'user_balance.dart';

class UserBalanceService {
  static Future<List<UserBalance>> getUserBalance(int userId) async {
    final response = await http.get(Uri.parse('http://localhost:5256/Users/UserBalance/$userId'));
    
    if (response.statusCode == 200) {
      final List<dynamic> data = json.decode(response.body);
      return data.map((json) => UserBalance.fromJson(json)).toList();
    } else {
      throw Exception('Failed to load user balance');
    }
  }
}
