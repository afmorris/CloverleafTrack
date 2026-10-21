-- OHSAA state team results for Lodi Cloverleaf read from the 1973-1995 PDFs (OCR checked against the printed standings): place and points; field size not recorded. Id-independent; insert-only.
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @m INT;
-- 1973 Boys: place 29, score not printed in the OCR
SET @m = (SELECT TOP 1 Id FROM Meets WHERE Name=N'1973 OHSAA State Track and Field Championships');
IF @m IS NULL THROW 50090, 'meet missing', 1;
IF NOT EXISTS (SELECT 1 FROM MeetTeamResults WHERE MeetId=@m AND Gender=1 AND OpponentMeetParticipantId IS NULL AND Deleted=0) INSERT INTO MeetTeamResults (MeetId, Gender, OpponentMeetParticipantId, OurScore, OpponentScore, Place, FieldSize) VALUES (@m, 1, NULL, NULL, NULL, 29, NULL);
-- 1982 Boys: place 48, 1
SET @m = (SELECT TOP 1 Id FROM Meets WHERE Name=N'1982 OHSAA Division I State Track and Field Championships');
IF @m IS NULL THROW 50090, 'meet missing', 1;
IF NOT EXISTS (SELECT 1 FROM MeetTeamResults WHERE MeetId=@m AND Gender=1 AND OpponentMeetParticipantId IS NULL AND Deleted=0) INSERT INTO MeetTeamResults (MeetId, Gender, OpponentMeetParticipantId, OurScore, OpponentScore, Place, FieldSize) VALUES (@m, 1, NULL, 1, NULL, 48, NULL);
-- 1986 Girls: place 52, 1
SET @m = (SELECT TOP 1 Id FROM Meets WHERE Name=N'1986 OHSAA Class AAA State Championships');
IF @m IS NULL THROW 50090, 'meet missing', 1;
IF NOT EXISTS (SELECT 1 FROM MeetTeamResults WHERE MeetId=@m AND Gender=2 AND OpponentMeetParticipantId IS NULL AND Deleted=0) INSERT INTO MeetTeamResults (MeetId, Gender, OpponentMeetParticipantId, OurScore, OpponentScore, Place, FieldSize) VALUES (@m, 2, NULL, 1, NULL, 52, NULL);
-- 1990 Girls: place 36, 3
SET @m = (SELECT TOP 1 Id FROM Meets WHERE Name=N'1990 OHSAA Division I State Track and Field Championships');
IF @m IS NULL THROW 50090, 'meet missing', 1;
IF NOT EXISTS (SELECT 1 FROM MeetTeamResults WHERE MeetId=@m AND Gender=2 AND OpponentMeetParticipantId IS NULL AND Deleted=0) INSERT INTO MeetTeamResults (MeetId, Gender, OpponentMeetParticipantId, OurScore, OpponentScore, Place, FieldSize) VALUES (@m, 2, NULL, 3, NULL, 36, NULL);
-- 1991 Boys: place 11, 10
SET @m = (SELECT TOP 1 Id FROM Meets WHERE Name=N'1991 OHSAA Division I State Track and Field Championships');
IF @m IS NULL THROW 50090, 'meet missing', 1;
IF NOT EXISTS (SELECT 1 FROM MeetTeamResults WHERE MeetId=@m AND Gender=1 AND OpponentMeetParticipantId IS NULL AND Deleted=0) INSERT INTO MeetTeamResults (MeetId, Gender, OpponentMeetParticipantId, OurScore, OpponentScore, Place, FieldSize) VALUES (@m, 1, NULL, 10, NULL, 11, NULL);
-- 1991 Girls: place 38, 2
SET @m = (SELECT TOP 1 Id FROM Meets WHERE Name=N'1991 OHSAA Division I State Track and Field Championships');
IF @m IS NULL THROW 50090, 'meet missing', 1;
IF NOT EXISTS (SELECT 1 FROM MeetTeamResults WHERE MeetId=@m AND Gender=2 AND OpponentMeetParticipantId IS NULL AND Deleted=0) INSERT INTO MeetTeamResults (MeetId, Gender, OpponentMeetParticipantId, OurScore, OpponentScore, Place, FieldSize) VALUES (@m, 2, NULL, 2, NULL, 38, NULL);
-- 1992 Boys: place 14, 10
SET @m = (SELECT TOP 1 Id FROM Meets WHERE Name=N'1992 OHSAA Division I State Track and Field Championships');
IF @m IS NULL THROW 50090, 'meet missing', 1;
IF NOT EXISTS (SELECT 1 FROM MeetTeamResults WHERE MeetId=@m AND Gender=1 AND OpponentMeetParticipantId IS NULL AND Deleted=0) INSERT INTO MeetTeamResults (MeetId, Gender, OpponentMeetParticipantId, OurScore, OpponentScore, Place, FieldSize) VALUES (@m, 1, NULL, 10, NULL, 14, NULL);
-- 1993 Boys: place 5, 26
SET @m = (SELECT TOP 1 Id FROM Meets WHERE Name=N'1993 OHSAA Division I State Track and Field Championships');
IF @m IS NULL THROW 50090, 'meet missing', 1;
IF NOT EXISTS (SELECT 1 FROM MeetTeamResults WHERE MeetId=@m AND Gender=1 AND OpponentMeetParticipantId IS NULL AND Deleted=0) INSERT INTO MeetTeamResults (MeetId, Gender, OpponentMeetParticipantId, OurScore, OpponentScore, Place, FieldSize) VALUES (@m, 1, NULL, 26, NULL, 5, NULL);
-- 1995 Boys: place 30, 6
SET @m = (SELECT TOP 1 Id FROM Meets WHERE Name=N'1995 OHSAA Division I State Track and Field Championships');
IF @m IS NULL THROW 50090, 'meet missing', 1;
IF NOT EXISTS (SELECT 1 FROM MeetTeamResults WHERE MeetId=@m AND Gender=1 AND OpponentMeetParticipantId IS NULL AND Deleted=0) INSERT INTO MeetTeamResults (MeetId, Gender, OpponentMeetParticipantId, OurScore, OpponentScore, Place, FieldSize) VALUES (@m, 1, NULL, 6, NULL, 30, NULL);
COMMIT TRANSACTION;
