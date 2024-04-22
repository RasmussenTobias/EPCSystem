import 'package:flutter/material.dart';
import 'user_service.dart';
import 'device_page.dart'; // Import DevicePage widget
import 'login_page.dart';

class FrontPage extends StatefulWidget {
  final Map<String, dynamic> selectedUser; // Pass both user ID and username as a map
  final List<Map<String, dynamic>> users; // Define _users list here

  FrontPage({required this.selectedUser, required this.users});

  @override
  _FrontPageState createState() => _FrontPageState();
}

class _FrontPageState extends State<FrontPage> {
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        automaticallyImplyLeading: false,
        title: Row(
          children: [
            Text(
              'Welcome to Electricity Trading Platform',
              style: TextStyle(
                fontSize: 18.0,
                fontWeight: FontWeight.bold,
                color: Colors.white,
              ),
            ),
            Spacer(), // Add space to separate the title and the switch account button
            Text(
              '${widget.selectedUser['username']}', // Show selected user's username
              style: TextStyle(
                fontSize: 16.0,
                color: Colors.white,
              ),
            ),
            IconButton(
              onPressed: () {
                // Navigate back to the login page
                Navigator.pushReplacement(
                  context,
                  MaterialPageRoute(builder: (context) => LoginScreen()),
                );
              },
              icon: Icon(Icons.account_circle, color: Colors.white),
            ),
          ],
        ),
        backgroundColor: Colors.green,
      ),
      body: Row(
        children: [
          Expanded(
            flex: 3,
            child: Column(
              children: [
                Expanded(
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      Text(
                        'Buy and sell electricity certificates',
                        style: TextStyle(fontSize: 24.0),
                      ),
                      SizedBox(height: 20.0),
                      ElevatedButton(
                        onPressed: () {
                          // Navigate to another page to buy/sell certificates
                        },
                        child: Text('Get Started'),
                      ),
                    ],
                  ),
                ),
                Expanded(
                  child: DevicePage(
                    userId: widget.selectedUser['id'],
                    username: widget.selectedUser['username'],
                  ), // Pass both user ID and username to DevicePage
                ),
              ],
            ),
          ),
          Expanded(
            flex: 7,
            child: Container(color: Colors.blue), // Placeholder for the middle column
          ),          
        ],
      ),
    );
  }
}
