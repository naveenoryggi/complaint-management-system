-- Find emails with direction = 1 (Inbound)
-- Direction enum: 0 = Outbound, 1 = Inbound

SELECT TOP 10
    em.Id,
    em.ComplaintId,
    em.Subject,
    em.FromEmail,
    em.Direction,
    CASE
        WHEN em.Direction = 0 THEN 'Outbound'
        WHEN em.Direction = 1 THEN 'Inbound'
        ELSE 'Unknown'
    END AS DirectionName,
    c.ComplaintNumber,
    c.Title AS ComplaintTitle
FROM EmailMessages em
INNER JOIN Complaints c ON em.ComplaintId = c.Id
WHERE em.IsDeleted = 0
ORDER BY em.ReceivedAt DESC;

-- Count by direction
SELECT
    Direction,
    CASE
        WHEN Direction = 0 THEN 'Outbound'
        WHEN Direction = 1 THEN 'Inbound'
        ELSE 'Unknown'
    END AS DirectionName,
    COUNT(*) AS Count
FROM EmailMessages
WHERE IsDeleted = 0
GROUP BY Direction;
