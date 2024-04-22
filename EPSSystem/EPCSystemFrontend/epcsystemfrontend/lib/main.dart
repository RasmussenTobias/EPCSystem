import 'package:flutter/material.dart';
import 'login_page.dart'; // Import LoginScreen widget

void main() {
  runApp(MyApp());
}

class MyApp extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Electricity Trading Platform',
      theme: ThemeData(
        primarySwatch: Colors.blue,
      ),
      home: LoginScreen(), // Navigate to the LoginScreen first
    );
  }
}
