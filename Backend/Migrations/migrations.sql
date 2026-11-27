CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;
ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `Courses` (
    `Id` int unsigned NOT NULL AUTO_INCREMENT,
    `Title` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `Description` varchar(1024) CHARACTER SET utf8mb4 NOT NULL,
    `AccessKey` varchar(128) CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_Courses` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Users` (
    `Id` int unsigned NOT NULL AUTO_INCREMENT,
    `Username` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    `Email` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `PasswordHash` varchar(256) CHARACTER SET utf8mb4 NOT NULL,
    `Role` int NOT NULL,
    CONSTRAINT `PK_Users` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Sections` (
    `Id` int unsigned NOT NULL AUTO_INCREMENT,
    `Title` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `Order` smallint unsigned NOT NULL,
    `CourseId` int unsigned NOT NULL,
    CONSTRAINT `PK_Sections` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Sections_Courses_CourseId` FOREIGN KEY (`CourseId`) REFERENCES `Courses` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `CourseInstructors` (
    `InstructorsId` int unsigned NOT NULL,
    `TeachingCoursesId` int unsigned NOT NULL,
    CONSTRAINT `PK_CourseInstructors` PRIMARY KEY (`InstructorsId`, `TeachingCoursesId`),
    CONSTRAINT `FK_CourseInstructors_Courses_TeachingCoursesId` FOREIGN KEY (`TeachingCoursesId`) REFERENCES `Courses` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_CourseInstructors_Users_InstructorsId` FOREIGN KEY (`InstructorsId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `Enrollments` (
    `Id` int unsigned NOT NULL AUTO_INCREMENT,
    `StudentId` int unsigned NOT NULL,
    `CourseId` int unsigned NOT NULL,
    `EnrolledAtUtc` datetime(6) NOT NULL,
    CONSTRAINT `PK_Enrollments` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Enrollments_Courses_CourseId` FOREIGN KEY (`CourseId`) REFERENCES `Courses` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Enrollments_Users_StudentId` FOREIGN KEY (`StudentId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `RefreshTokens` (
    `Id` int unsigned NOT NULL AUTO_INCREMENT,
    `TokenHash` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `JwtId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `UserId` int unsigned NOT NULL,
    `ExpiresAtUtc` datetime(6) NOT NULL,
    `CreatedAtUtc` datetime(6) NOT NULL,
    `CreatedByIp` longtext CHARACTER SET utf8mb4 NULL,
    `RevokedAtUtc` datetime(6) NULL,
    `RevokedByIp` longtext CHARACTER SET utf8mb4 NULL,
    `ReasonRevoked` longtext CHARACTER SET utf8mb4 NULL,
    `ReplacedByTokenHash` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_RefreshTokens` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_RefreshTokens_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `Lessons` (
    `Id` int unsigned NOT NULL AUTO_INCREMENT,
    `Title` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `Content` longtext CHARACTER SET utf8mb4 NOT NULL,
    `VideoUrl` varchar(512) CHARACTER SET utf8mb4 NULL,
    `Order` smallint unsigned NOT NULL,
    `SectionId` int unsigned NOT NULL,
    CONSTRAINT `PK_Lessons` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Lessons_Sections_SectionId` FOREIGN KEY (`SectionId`) REFERENCES `Sections` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `Quizzes` (
    `Id` int unsigned NOT NULL AUTO_INCREMENT,
    `Title` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `SectionId` int unsigned NOT NULL,
    `MaxAttempts` smallint unsigned NULL,
    `TimeLimitSeconds` int unsigned NULL,
    `PassingThresholdPercent` decimal(5,2) NOT NULL,
    `RandomizeQuestions` tinyint(1) NOT NULL,
    `RandomizeAnswers` tinyint(1) NOT NULL,
    CONSTRAINT `PK_Quizzes` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Quizzes_Sections_SectionId` FOREIGN KEY (`SectionId`) REFERENCES `Sections` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `LessonAttachments` (
    `Id` int unsigned NOT NULL AUTO_INCREMENT,
    `FileName` varchar(256) CHARACTER SET utf8mb4 NOT NULL,
    `Url` varchar(2048) CHARACTER SET utf8mb4 NOT NULL,
    `ContentType` varchar(100) CHARACTER SET utf8mb4 NULL,
    `SizeBytes` bigint NULL,
    `LessonId` int unsigned NOT NULL,
    CONSTRAINT `PK_LessonAttachments` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_LessonAttachments_Lessons_LessonId` FOREIGN KEY (`LessonId`) REFERENCES `Lessons` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `StudentLessonProgresses` (
    `Id` int unsigned NOT NULL AUTO_INCREMENT,
    `StudentId` int unsigned NOT NULL,
    `LessonId` int unsigned NOT NULL,
    `IsCompleted` tinyint(1) NOT NULL,
    `CompletedAtUtc` datetime(6) NULL,
    CONSTRAINT `PK_StudentLessonProgresses` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_StudentLessonProgresses_Lessons_LessonId` FOREIGN KEY (`LessonId`) REFERENCES `Lessons` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_StudentLessonProgresses_Users_StudentId` FOREIGN KEY (`StudentId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `Questions` (
    `Id` int unsigned NOT NULL AUTO_INCREMENT,
    `Text` varchar(2048) CHARACTER SET utf8mb4 NOT NULL,
    `ImageUrl` varchar(2048) CHARACTER SET utf8mb4 NULL,
    `Type` int NOT NULL,
    `Points` smallint unsigned NOT NULL,
    `QuizId` int unsigned NOT NULL,
    CONSTRAINT `PK_Questions` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Questions_Quizzes_QuizId` FOREIGN KEY (`QuizId`) REFERENCES `Quizzes` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `QuizAttempts` (
    `Id` int unsigned NOT NULL AUTO_INCREMENT,
    `QuizId` int unsigned NOT NULL,
    `StudentId` int unsigned NOT NULL,
    `StartedAtUtc` datetime(6) NOT NULL,
    `SubmittedAtUtc` datetime(6) NULL,
    `ScorePercent` decimal(5,2) NOT NULL,
    `IsPassed` tinyint(1) NOT NULL,
    CONSTRAINT `PK_QuizAttempts` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_QuizAttempts_Quizzes_QuizId` FOREIGN KEY (`QuizId`) REFERENCES `Quizzes` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_QuizAttempts_Users_StudentId` FOREIGN KEY (`StudentId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `AnswerOptions` (
    `Id` int unsigned NOT NULL AUTO_INCREMENT,
    `Text` varchar(1024) CHARACTER SET utf8mb4 NOT NULL,
    `ImageUrl` varchar(2048) CHARACTER SET utf8mb4 NULL,
    `IsCorrect` tinyint(1) NOT NULL,
    `QuestionId` int unsigned NOT NULL,
    CONSTRAINT `PK_AnswerOptions` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_AnswerOptions_Questions_QuestionId` FOREIGN KEY (`QuestionId`) REFERENCES `Questions` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `StudentAnswers` (
    `Id` int unsigned NOT NULL AUTO_INCREMENT,
    `QuizAttemptId` int unsigned NOT NULL,
    `QuestionId` int unsigned NOT NULL,
    `SelectedOptionIdsJson` varchar(1024) CHARACTER SET utf8mb4 NOT NULL,
    `IsFullyCorrect` tinyint(1) NOT NULL,
    `AwardedPoints` smallint unsigned NOT NULL,
    CONSTRAINT `PK_StudentAnswers` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_StudentAnswers_Questions_QuestionId` FOREIGN KEY (`QuestionId`) REFERENCES `Questions` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_StudentAnswers_QuizAttempts_QuizAttemptId` FOREIGN KEY (`QuizAttemptId`) REFERENCES `QuizAttempts` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_AnswerOptions_QuestionId` ON `AnswerOptions` (`QuestionId`);

CREATE INDEX `IX_CourseInstructors_TeachingCoursesId` ON `CourseInstructors` (`TeachingCoursesId`);

CREATE INDEX `IX_Enrollments_CourseId` ON `Enrollments` (`CourseId`);

CREATE UNIQUE INDEX `IX_Enrollments_StudentId_CourseId` ON `Enrollments` (`StudentId`, `CourseId`);

CREATE INDEX `IX_LessonAttachments_LessonId` ON `LessonAttachments` (`LessonId`);

CREATE INDEX `IX_Lessons_SectionId_Order` ON `Lessons` (`SectionId`, `Order`);

CREATE INDEX `IX_Questions_QuizId` ON `Questions` (`QuizId`);

CREATE INDEX `IX_QuizAttempts_QuizId` ON `QuizAttempts` (`QuizId`);

CREATE INDEX `IX_QuizAttempts_StudentId_QuizId` ON `QuizAttempts` (`StudentId`, `QuizId`);

CREATE INDEX `IX_Quizzes_SectionId` ON `Quizzes` (`SectionId`);

CREATE INDEX `IX_RefreshTokens_JwtId` ON `RefreshTokens` (`JwtId`);

CREATE UNIQUE INDEX `IX_RefreshTokens_TokenHash` ON `RefreshTokens` (`TokenHash`);

CREATE INDEX `IX_RefreshTokens_UserId` ON `RefreshTokens` (`UserId`);

CREATE INDEX `IX_Sections_CourseId_Order` ON `Sections` (`CourseId`, `Order`);

CREATE INDEX `IX_StudentAnswers_QuestionId` ON `StudentAnswers` (`QuestionId`);

CREATE UNIQUE INDEX `IX_StudentAnswers_QuizAttemptId_QuestionId` ON `StudentAnswers` (`QuizAttemptId`, `QuestionId`);

CREATE INDEX `IX_StudentLessonProgresses_LessonId` ON `StudentLessonProgresses` (`LessonId`);

CREATE UNIQUE INDEX `IX_StudentLessonProgresses_StudentId_LessonId` ON `StudentLessonProgresses` (`StudentId`, `LessonId`);

CREATE UNIQUE INDEX `IX_Users_Email` ON `Users` (`Email`);

CREATE UNIQUE INDEX `IX_Users_Username` ON `Users` (`Username`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261108224026_Init', '10.0.15');

COMMIT;

START TRANSACTION;
ALTER TABLE `Users` MODIFY COLUMN `Role` tinyint unsigned NOT NULL;

ALTER TABLE `RefreshTokens` MODIFY COLUMN `TokenHash` varchar(64) CHARACTER SET utf8mb4 NOT NULL;

ALTER TABLE `RefreshTokens` MODIFY COLUMN `RevokedByIp` varchar(45) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `RefreshTokens` MODIFY COLUMN `ReplacedByTokenHash` varchar(64) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `RefreshTokens` MODIFY COLUMN `ReasonRevoked` varchar(128) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `RefreshTokens` MODIFY COLUMN `JwtId` varchar(32) CHARACTER SET utf8mb4 NOT NULL;

ALTER TABLE `RefreshTokens` MODIFY COLUMN `CreatedByIp` varchar(45) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `RefreshTokens` ADD `DeviceName` varchar(100) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `RefreshTokens` ADD `TokenFamilyId` char(36) COLLATE ascii_general_ci NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE `RefreshTokens` ADD `UserAgent` varchar(512) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `Questions` MODIFY COLUMN `Type` tinyint unsigned NOT NULL;

CREATE INDEX `IX_RefreshTokens_TokenFamilyId` ON `RefreshTokens` (`TokenFamilyId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261110182434_Auth', '10.0.15');

COMMIT;

START TRANSACTION;
ALTER TABLE `RefreshTokens` ADD `AccessTokenExpiresAtUtc` datetime(6) NULL;

UPDATE `RefreshTokens` SET `AccessTokenExpiresAtUtc` = `ExpiresAtUtc` WHERE `AccessTokenExpiresAtUtc` IS NULL

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261110223549_StoreAccessTokenExpiration', '10.0.15');

COMMIT;

START TRANSACTION;
ALTER TABLE `StudentAnswers` ADD `SelectedOptionIds` longtext CHARACTER SET utf8mb4 NULL;

UPDATE `StudentAnswers`
SET `SelectedOptionIds` = `SelectedOptionIdsJson`
WHERE `SelectedOptionIdsJson` IS NOT NULL AND `SelectedOptionIdsJson` <> '';

ALTER TABLE `StudentAnswers` DROP COLUMN `SelectedOptionIdsJson`;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261122172914_RemoveJsonFromAnswers', '10.0.15');

COMMIT;

