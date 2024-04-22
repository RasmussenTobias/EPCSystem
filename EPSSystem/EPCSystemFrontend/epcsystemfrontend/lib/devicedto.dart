class DeviceDto {
  final int userId;
  final String deviceName;
  final String location;

  DeviceDto({
    required this.userId,
    required this.deviceName,
    required this.location,
  });

  // Add a method to convert to JSON
  Map<String, dynamic> toJson() {
    return {
      'userId': userId,
      'deviceName': deviceName,
      'location': location,
    };
  }
}