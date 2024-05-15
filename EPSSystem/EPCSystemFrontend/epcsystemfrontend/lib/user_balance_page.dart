import 'package:flutter/material.dart';
import 'user_balance_service.dart'; // Import UserBalanceService
import 'user_balance.dart'; // Import UserBalance model
import 'transfer_page.dart'; // Import TransferPage

class UserBalancePage extends StatefulWidget {
  final int userId;

  UserBalancePage({required this.userId});

  @override
  _UserBalancePageState createState() => _UserBalancePageState();
}

class _UserBalancePageState extends State<UserBalancePage> {
  late Future<List<UserBalance>> _userBalanceFuture;

  @override
  void initState() {
    super.initState();
    _userBalanceFuture = UserBalanceService.getUserBalance(widget.userId);
  }

  // Callback function to update account balances
  void _updateBalance() {
    setState(() {
      _userBalanceFuture = UserBalanceService.getUserBalance(widget.userId);
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Row(
          children: [
            Expanded(
              child: Text('Account Balances'),
            ),
            TextButton(
              onPressed: () {
                // Navigate to TransferPage and pass the callback function
                Navigator.push(
                  context,
                  MaterialPageRoute(
                    builder: (context) => TransferPage(
                      fromUserId: widget.userId,
                      onUpdateBalance: _updateBalance, // Pass the callback function
                    ),
                  ),
                );
              },
              child: Text(
                'Transfer',
                style: TextStyle(color: Colors.white),
              ),
              style: ButtonStyle(
                backgroundColor: MaterialStateProperty.all<Color>(Colors.green),
              ),
            ),
          ],
        ),
      ),
      body: FutureBuilder(
        future: _userBalanceFuture,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return Center(
              child: CircularProgressIndicator(),
            );
          } else if (snapshot.hasError) {
            return Center(
              child: Text('Error: ${snapshot.error}'),
            );
          } else {
            List<UserBalance> userBalances = snapshot.data as List<UserBalance>;
            return ListView.builder(
              itemCount: userBalances.length,
              itemBuilder: (context, index) {
                return _buildListItem(userBalances[index]);
              },
            );
          }
        },
      ),
    );
  }

  Widget _buildListItem(UserBalance balance) {
    IconData iconData;
    String currencyType;

    // Determine icon and currency type based on power type and device type
    if (balance.deviceType == 'Wind') {
      iconData = Icons.ac_unit; // Placeholder icon for Wind power type
      currencyType = 'Wind Power';
    } else if (balance.deviceType == 'Solar') {
      iconData = Icons.wb_sunny; // Placeholder icon for Solar power type
      currencyType = 'Solar Power';
    } else {
      // Add more conditions for other power types if needed
      iconData = Icons.money; // Placeholder icon for Unknown power type
      currencyType = 'Unknown';
    }

    return Column(
      children: [
        Card(
          elevation: 0, // Set elevation to 0
          margin: EdgeInsets.symmetric(vertical: 8, horizontal: 4),
          child: ListTile(
            leading: Icon(iconData),
            title: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    Text(
                      balance.deviceName,
                      style: TextStyle(fontWeight: FontWeight.bold),
                    ),
                    SizedBox(width: 8),
                  ],
                ),
                SizedBox(height: 4), // Add some space between device name and ElectricityProductionId
                Text(
                  '${balance.electricityProductionId}',
                  style: TextStyle(fontSize: 16), // Bigger font size for ElectricityProductionId
                ),
              ],
            ),
            trailing: Text(
              '${balance.balance}',
              style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16), // Bigger font size for balance
            ),
          ),
        ),
        Divider(), // Add Divider widget between each Card
      ],
    );
  }
}
