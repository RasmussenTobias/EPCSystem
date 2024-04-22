import 'package:flutter/material.dart';
import 'user_service.dart';
import 'front_page.dart'; // Import FrontPage widget

class LoginScreen extends StatefulWidget {
  @override
  _LoginScreenState createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  List<Map<String, dynamic>> _users = [];

  @override
  void initState() {
    super.initState();
    _fetchUsers();
  }

  Future<void> _fetchUsers() async {
    try {
      List<Map<String, dynamic>> users = await UserService.getUsers();
      setState(() {
        _users = users;
      });
    } catch (e) {
      print('Failed to fetch users: $e');
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text('Login'),
      ),
      body: ListView.builder(
        itemCount: _users.length,
        itemBuilder: (context, index) {
          return ListTile(
            title: Text(_users[index]['username']),
            onTap: () {
              Navigator.push(
                context,
                MaterialPageRoute(
                  builder: (context) => FrontPage(
                    selectedUser: _users[index],
                    users: _users,
                  ), // Pass both user ID and username to FrontPage
                ),
              );
            },
          );
        },
      ),
    );
  }
}