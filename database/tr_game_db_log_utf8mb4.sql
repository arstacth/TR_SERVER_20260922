/*
 Navicat Premium Data Transfer

 Source Server         : arstacth
 Source Server Type    : MySQL
 Source Server Version : 100428
 Source Host           : arstacth
 Source Schema         : tr_game_db_log

 Target Server Type    : MySQL
 Target Server Version : 100428
 File Encoding         : 65001

 Date: 23/09/2026 02:08:33
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for log_buyitem
-- ----------------------------
DROP TABLE IF EXISTS `log_buyitem`;
CREATE TABLE `log_buyitem`  (
  `fdNum` bigint NOT NULL AUTO_INCREMENT,
  `fdDateTime` datetime NOT NULL DEFAULT current_timestamp,
  `fdUserNum` int NOT NULL,
  `fdItemDescNum` int NOT NULL,
  PRIMARY KEY (`fdNum`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of log_buyitem
-- ----------------------------

-- ----------------------------
-- Table structure for log_capsulemachineresult
-- ----------------------------
DROP TABLE IF EXISTS `log_capsulemachineresult`;
CREATE TABLE `log_capsulemachineresult`  (
  `fdSendUserNum` int NOT NULL,
  `fdGotDate` datetime NOT NULL DEFAULT current_timestamp,
  `fdMachineNum` int NOT NULL,
  `fdItemNum` int NOT NULL,
  `fdReceiveUserNum` int NOT NULL,
  `fdMemo` varchar(1024) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `fdErrorDesc` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of log_capsulemachineresult
-- ----------------------------

-- ----------------------------
-- Table structure for log_gameroomresult
-- ----------------------------
DROP TABLE IF EXISTS `log_gameroomresult`;
CREATE TABLE `log_gameroomresult`  (
  `fdMap` int NOT NULL DEFAULT 0,
  `fdRecord` int NOT NULL DEFAULT 0,
  `fdTotalExp` int NULL DEFAULT NULL,
  `fdTotalTR` int NULL DEFAULT NULL,
  `fdPlayerCount` int NOT NULL DEFAULT 0,
  `fdDate` datetime NOT NULL DEFAULT current_timestamp
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of log_gameroomresult
-- ----------------------------

-- ----------------------------
-- Table structure for log_giftitem
-- ----------------------------
DROP TABLE IF EXISTS `log_giftitem`;
CREATE TABLE `log_giftitem`  (
  `fdNum` bigint NOT NULL AUTO_INCREMENT,
  `fdSendUserNum` int NULL DEFAULT NULL,
  `fdSendNickname` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `fdReceiveUserNum` int NOT NULL,
  `fdGiftItemDescNum` int NOT NULL,
  `fdDateTime` datetime NULL DEFAULT current_timestamp,
  `fdMemo` varchar(1024) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `fdExpireDate` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`fdNum`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of log_giftitem
-- ----------------------------

-- ----------------------------
-- Table structure for log_shoutitem
-- ----------------------------
DROP TABLE IF EXISTS `log_shoutitem`;
CREATE TABLE `log_shoutitem`  (
  `fdNum` bigint NOT NULL AUTO_INCREMENT,
  `fdUserNum` int NOT NULL,
  `fdShoutItemNum` int NOT NULL,
  `fdShoutMsg` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `fdDateTime` datetime NOT NULL DEFAULT current_timestamp,
  PRIMARY KEY (`fdNum`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of log_shoutitem
-- ----------------------------

-- ----------------------------
-- Table structure for log_useluckybag
-- ----------------------------
DROP TABLE IF EXISTS `log_useluckybag`;
CREATE TABLE `log_useluckybag`  (
  `fdNum` bigint NOT NULL AUTO_INCREMENT,
  `fdUserNum` int NOT NULL,
  `fdUseItemDescNum` int NOT NULL,
  `fdIndex` int NOT NULL,
  `fdCount` int NOT NULL,
  `fdGiveItemDescNum` int NOT NULL,
  `fdDatetime` datetime NOT NULL DEFAULT current_timestamp,
  PRIMARY KEY (`fdNum`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of log_useluckybag
-- ----------------------------

-- ----------------------------
-- Table structure for log_usergameresult
-- ----------------------------
DROP TABLE IF EXISTS `log_usergameresult`;
CREATE TABLE `log_usergameresult`  (
  `fdDateTime` datetime NOT NULL DEFAULT current_timestamp,
  `fdUsernum` int NULL DEFAULT NULL,
  `fdExp` bigint NULL DEFAULT NULL,
  `fdGameMoney` bigint NULL DEFAULT NULL,
  `fdLadderPoint` int NULL DEFAULT NULL,
  `fdMapNum` int NULL DEFAULT NULL,
  `fdGuildMatchPoint` int NULL DEFAULT NULL,
  `fdGuildPoint` int NULL DEFAULT NULL,
  `fdLapTime` int NULL DEFAULT NULL,
  `fdWinTeam` tinyint(1) NULL DEFAULT NULL,
  `fdTimeOut` tinyint(1) NULL DEFAULT NULL,
  `fdLevel` int NULL DEFAULT NULL
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of log_usergameresult
-- ----------------------------

-- ----------------------------
-- Table structure for log_userloginandlogout
-- ----------------------------
DROP TABLE IF EXISTS `log_userloginandlogout`;
CREATE TABLE `log_userloginandlogout`  (
  `fdNum` bigint NOT NULL AUTO_INCREMENT,
  `fdUserNum` int NOT NULL,
  `fdUID` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `fdNickName` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT '_nonick_',
  `fdLoginDateTime` datetime NOT NULL,
  `fdLogoutDateTime` datetime NULL DEFAULT current_timestamp,
  `fdServerNum` smallint NOT NULL DEFAULT 1,
  `fdExp` bigint NOT NULL,
  `fdGameMoney` bigint NOT NULL,
  `fdFarmPoint` int NOT NULL,
  `fdLadderPoint` int NOT NULL,
  `fdIP` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`fdNum`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of log_userloginandlogout
-- ----------------------------

SET FOREIGN_KEY_CHECKS = 1;
