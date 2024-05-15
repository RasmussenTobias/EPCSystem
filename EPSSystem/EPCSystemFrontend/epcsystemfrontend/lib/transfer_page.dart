import 'package:flutter/material.dart';
import 'user_certificate_service.dart';
import 'transfer_service.dart';
import 'user_balance_service.dart';
import 'dart:convert';

class TransferPage extends StatefulWidget {
  final int fromUserId;
  final Function onUpdateBalance;

  TransferPage({required this.fromUserId, required this.onUpdateBalance});

  @override
  _TransferPageState createState() => _TransferPageState();
}

class _TransferPageState extends State<TransferPage> {
  late TextEditingController _toUserIdController;
  List<Map<String, dynamic>> _userCertificates = [];
  List<int> _selectedCertificateIndices = [];
  List<TextEditingController> _amountControllers = [];

  @override
  void initState() {
    super.initState();
    _toUserIdController = TextEditingController();
    _fetchUserCertificates();
  }

  Future<void> _fetchUserCertificates() async {
    List<Map<String, dynamic>>? certificates = await CertificateService.getUserCertificates(widget.fromUserId);
    if (certificates != null) {
      setState(() {
        _userCertificates = certificates;
        _amountControllers = List.generate(certificates.length, (index) => TextEditingController());
      });
    } else {
      // Handle error fetching certificates
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text('Transfer'),
      ),
      body: Padding(
        padding: EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Text(
              'From User ID: ${widget.fromUserId}',
              style: TextStyle(fontWeight: FontWeight.bold),
            ),
            SizedBox(height: 16.0),
            TextFormField(
              controller: _toUserIdController,
              decoration: InputDecoration(labelText: 'To User ID'),
              keyboardType: TextInputType.number,
            ),
            SizedBox(height: 16.0),
            DropdownButtonFormField<Map<String, dynamic>>(
              value: null,
              items: _userCertificates.asMap().entries.map((entry) {
                int index = entry.key;
                Map<String, dynamic> certificate = entry.value;
                return DropdownMenuItem<Map<String, dynamic>>(
                  value: certificate,
                  child: Text(
                      'Certificate ${certificate['id']} - ElectricityProductionID: ${certificate['electricityProductionId']} - Volume: ${certificate['volume']}'),
                );
              }).toList(),
              onChanged: (Map<String, dynamic>? value) {
                if (value != null) {
                  setState(() {
                    _selectedCertificateIndices.add(_userCertificates.indexOf(value));
                  });
                }
              },
              decoration: InputDecoration(labelText: 'Select Certificate'),
            ),
            SizedBox(height: 16.0),
            Expanded(
              child: ListView.builder(
                itemCount: _selectedCertificateIndices.length,
                itemBuilder: (context, index) {
                  int certificateIndex = _selectedCertificateIndices[index];
                  return Row(
                    children: [
                      Expanded(
                        child: TextFormField(
                          controller: _amountControllers[certificateIndex],
                          decoration: InputDecoration(
                              labelText: 'Amount to Transfer for Certificate ${_userCertificates[certificateIndex]['id']}'),
                          keyboardType: TextInputType.number,
                        ),
                      ),
                      IconButton(
                        onPressed: () {
                          setState(() {
                            _selectedCertificateIndices.removeAt(index);
                          });
                        },
                        icon: Icon(Icons.remove),
                      ),
                    ],
                  );
                },
              ),
            ),
            SizedBox(height: 16.0),
            ElevatedButton(
              onPressed: () async {
                await _sendTransfer();
              },
              child: Text('Send Transfer'),
            ),
          ],
        ),
      ),
    );
  }

  Future<void> _sendTransfer() async {
    int toUserId = int.tryParse(_toUserIdController.text) ?? 0;
    List<Map<String, dynamic>> transfers = [];

    for (int index in _selectedCertificateIndices) {
      int amount = int.tryParse(_amountControllers[index].text) ?? 0;
      if (amount > 0) {
        transfers.add({
          'certificateId': _userCertificates[index]['id'],
          'amount': amount,
        });
      }
    }

    int fromUserId = widget.fromUserId;

    String result = await TransferService.sendTransfer(
      fromUserId: fromUserId,
      toUserId: toUserId,
      transfers: transfers,
    );

    await showDialog(
      context: context,
      builder: (BuildContext context) {
        return AlertDialog(
          title: Text(result.startsWith('Transfer successful') ? 'Success' : 'Error'),
          content: Text(result),
          actions: [
            TextButton(
              onPressed: () {
                Navigator.of(context).pop();
                if (result.startsWith('Transfer successful')) {
                  widget.onUpdateBalance();
                  Navigator.pop(context);
                }
              },
              child: Text('OK'),
            ),
          ],
        );
      },
    );
  }

  @override
  void dispose() {
    _toUserIdController.dispose();
    _amountControllers.forEach((controller) {
      controller.dispose();
    });
    super.dispose();
  }
}
