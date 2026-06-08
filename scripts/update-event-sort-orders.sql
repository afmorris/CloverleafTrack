-- Reassigns SortOrder for all events to implement the display ordering:
--   Sprints (< 100) → Hurdles (100–149) → Distance (150–299)
--   → Throws (300–429) → Jumps (430+)
-- Relay groups are interspersed immediately after their parent discipline.
-- Indoor events use the same slot ranges so the leaderboard header logic is identical.

UPDATE Events SET SortOrder = CASE Id

  -- =========================================================================
  -- BOYS OUTDOOR
  -- =========================================================================

  -- Sprints
  WHEN 45  THEN 10   -- 100M
  WHEN 65  THEN 20   -- 200M
  WHEN 37  THEN 30   -- 400M
  -- Seated sprints (adaptive)
  WHEN 58  THEN 35   -- Seated 100M
  WHEN 60  THEN 36   -- Seated 200M
  WHEN 66  THEN 37   -- Seated 400M
  -- Sprint relays
  WHEN 99  THEN 40   -- 4x100M Relay
  WHEN 114 THEN 50   -- 4x200M Relay
  WHEN 108 THEN 60   -- 4x400M Relay
  WHEN 100 THEN 70   -- 800M Sprint Medley
  WHEN 124 THEN 75   -- Swedish Relay
  WHEN 97  THEN 80   -- 4x100M Weight Relay

  -- Hurdles
  WHEN 55  THEN 100  -- 110M Hurdles
  WHEN 42  THEN 110  -- 300M Hurdles
  -- Hurdle relays
  WHEN 109 THEN 120  -- 4x110M Shuttle Hurdles
  WHEN 102 THEN 130  -- 4x300M Shuttle Hurdles

  -- Distance
  WHEN 34  THEN 150  -- 800M
  WHEN 43  THEN 155  -- Seated 800M
  WHEN 56  THEN 160  -- 1600M
  WHEN 62  THEN 170  -- 3200M
  WHEN 36  THEN 180  -- 2K Steeple Chase
  -- Distance relays
  WHEN 118 THEN 200  -- 4x800M Relay
  WHEN 105 THEN 210  -- 4x1600M Relay
  WHEN 101 THEN 220  -- 4K Distance Medley
  WHEN 133 THEN 230  -- 1600M Intermediate Medley
  WHEN 135 THEN 240  -- 2400M Intermediate Medley
  WHEN 126 THEN 250  -- 4x400M Steeple Relay

  -- Throws
  WHEN 22  THEN 300  -- Shot Put
  WHEN 8   THEN 305  -- Seated Shot Put
  WHEN 88  THEN 310  -- 2-Person Shot Put Relay
  WHEN 69  THEN 320  -- 3-Person Shot Put Relay
  WHEN 13  THEN 340  -- Discus
  WHEN 96  THEN 350  -- 2-Person Discus Relay
  WHEN 72  THEN 360  -- 3-Person Discus Relay
  WHEN 7   THEN 380  -- Hammer Throw
  WHEN 86  THEN 390  -- 2-Person Hammer Throw Relay
  WHEN 71  THEN 400  -- 3-Person Hammer Throw Relay

  -- Jumps
  WHEN 1   THEN 430  -- Long Jump
  WHEN 92  THEN 440  -- 2-Person Long Jump Relay
  WHEN 73  THEN 450  -- 3-Person Long Jump Relay
  WHEN 14  THEN 470  -- High Jump
  WHEN 90  THEN 480  -- 2-Person High Jump Relay
  WHEN 81  THEN 490  -- 3-Person High Jump Relay
  WHEN 20  THEN 510  -- Triple Jump
  WHEN 94  THEN 520  -- 2-Person Triple Jump Relay
  WHEN 76  THEN 530  -- 3-Person Triple Jump Relay
  WHEN 3   THEN 550  -- Pole Vault
  WHEN 84  THEN 560  -- 2-Person Pole Vault Relay
  WHEN 78  THEN 570  -- 3-Person Pole Vault Relay

  -- =========================================================================
  -- GIRLS OUTDOOR
  -- =========================================================================

  -- Sprints
  WHEN 49  THEN 10   -- 100M
  WHEN 39  THEN 20   -- 200M
  WHEN 46  THEN 30   -- 400M
  -- Seated sprints (adaptive)
  WHEN 47  THEN 35   -- Seated 100M
  WHEN 33  THEN 36   -- Seated 200M
  WHEN 51  THEN 37   -- Seated 400M
  -- Sprint relays
  WHEN 106 THEN 40   -- 4x100M Relay
  WHEN 111 THEN 50   -- 4x200M Relay
  WHEN 115 THEN 60   -- 4x400M Relay
  WHEN 113 THEN 70   -- 800M Sprint Medley
  WHEN 123 THEN 75   -- Swedish Relay
  WHEN 117 THEN 80   -- 4x100M Weight Relay

  -- Hurdles
  WHEN 52  THEN 100  -- 100M Hurdles
  WHEN 29  THEN 110  -- 300M Hurdles
  -- Hurdle relays
  WHEN 119 THEN 120  -- 4x100M Shuttle Hurdles
  WHEN 98  THEN 130  -- 4x300M Shuttle Hurdles

  -- Distance
  WHEN 50  THEN 150  -- 800M
  WHEN 67  THEN 155  -- Seated 800M
  WHEN 41  THEN 160  -- 1600M
  WHEN 35  THEN 170  -- 3200M
  WHEN 44  THEN 180  -- 2K Steeple Chase
  -- Distance relays
  WHEN 120 THEN 200  -- 4x800M Relay
  WHEN 110 THEN 210  -- 4x1600M Relay
  WHEN 103 THEN 220  -- 4K Distance Medley
  WHEN 134 THEN 230  -- 1600M Intermediate Medley
  WHEN 136 THEN 240  -- 2400M Intermediate Medley
  WHEN 125 THEN 250  -- 4x400M Steeple Relay

  -- Throws
  WHEN 2   THEN 300  -- Shot Put
  WHEN 10  THEN 305  -- Seated Shot Put
  WHEN 87  THEN 310  -- 2-Person Shot Put Relay
  WHEN 79  THEN 320  -- 3-Person Shot Put Relay
  WHEN 24  THEN 340  -- Discus
  WHEN 95  THEN 350  -- 2-Person Discus Relay
  WHEN 74  THEN 360  -- 3-Person Discus Relay
  WHEN 21  THEN 380  -- Hammer Throw
  WHEN 85  THEN 390  -- 2-Person Hammer Throw Relay
  WHEN 80  THEN 400  -- 3-Person Hammer Throw Relay

  -- Jumps
  WHEN 25  THEN 430  -- Long Jump
  WHEN 91  THEN 440  -- 2-Person Long Jump Relay
  WHEN 77  THEN 450  -- 3-Person Long Jump Relay
  WHEN 11  THEN 470  -- High Jump
  WHEN 89  THEN 480  -- 2-Person High Jump Relay
  WHEN 70  THEN 490  -- 3-Person High Jump Relay
  WHEN 15  THEN 510  -- Triple Jump
  WHEN 93  THEN 520  -- 2-Person Triple Jump Relay
  WHEN 75  THEN 530  -- 3-Person Triple Jump Relay
  WHEN 17  THEN 550  -- Pole Vault
  WHEN 83  THEN 560  -- 2-Person Pole Vault Relay
  WHEN 82  THEN 570  -- 3-Person Pole Vault Relay

  -- =========================================================================
  -- BOYS INDOOR  (same slot ranges as outdoor for consistent group detection)
  -- =========================================================================

  -- Sprints
  WHEN 30  THEN 10   -- 60M
  WHEN 57  THEN 20   -- 200M
  WHEN 68  THEN 30   -- 400M
  -- Sprint relays
  WHEN 121 THEN 40   -- 4x200M Relay
  WHEN 116 THEN 50   -- 4x400M Relay

  -- Hurdles
  WHEN 54  THEN 110  -- 60M Hurdles

  -- Distance
  WHEN 63  THEN 150  -- 800M
  WHEN 38  THEN 160  -- 1600M
  WHEN 64  THEN 170  -- 3200M
  -- Distance relays
  WHEN 104 THEN 200  -- 4x800M Relay

  -- Throws
  WHEN 23  THEN 300  -- Shot Put
  WHEN 6   THEN 305  -- Seated Shot Put

  -- Jumps
  WHEN 9   THEN 430  -- Long Jump
  WHEN 28  THEN 470  -- High Jump
  WHEN 26  THEN 510  -- Triple Jump
  WHEN 5   THEN 550  -- Pole Vault

  -- =========================================================================
  -- GIRLS INDOOR  (same slot ranges as outdoor for consistent group detection)
  -- =========================================================================

  -- Sprints
  WHEN 48  THEN 10   -- 60M
  WHEN 59  THEN 20   -- 200M
  WHEN 53  THEN 30   -- 400M
  -- Sprint relays
  WHEN 122 THEN 40   -- 4x200M Relay
  WHEN 107 THEN 50   -- 4x400M Relay

  -- Hurdles
  WHEN 40  THEN 110  -- 60M Hurdles

  -- Distance
  WHEN 61  THEN 150  -- 800M
  WHEN 32  THEN 160  -- 1600M
  WHEN 31  THEN 170  -- 3200M
  -- Distance relays
  WHEN 112 THEN 200  -- 4x800M Relay

  -- Throws
  WHEN 12  THEN 300  -- Shot Put
  WHEN 4   THEN 305  -- Seated Shot Put

  -- Jumps
  WHEN 16  THEN 430  -- Long Jump
  WHEN 27  THEN 470  -- High Jump
  WHEN 18  THEN 510  -- Triple Jump
  WHEN 19  THEN 550  -- Pole Vault

  -- =========================================================================
  -- MIXED OUTDOOR
  -- =========================================================================

  WHEN 127 THEN 50   -- Mixed 4x200M Relay   (Sprints)
  WHEN 128 THEN 60   -- Mixed 4x400M Relay   (Sprints)
  WHEN 129 THEN 200  -- Mixed 4x800M Relay   (Distance)

  ELSE SortOrder
END;
