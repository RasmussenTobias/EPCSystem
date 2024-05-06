class UserBalance {
  final int userId;
  final int electricityProductionId;
  final int balance;
  final String powerType;
  final String deviceType;
  final String deviceName;

  UserBalance({
    required this.userId,
    required this.electricityProductionId,
    required this.balance,
    required this.powerType,
    required this.deviceType,
    required this.deviceName
  });

  factory UserBalance.fromJson(Map<String, dynamic> json) {
    return UserBalance(
      userId: json['userId'],
      electricityProductionId: json['electricityProductionId'],
      balance: json['balance'],
      powerType: json['powerType'],
      deviceType: json['deviceType'],
      deviceName: json['deviceName']
    );
  }
}