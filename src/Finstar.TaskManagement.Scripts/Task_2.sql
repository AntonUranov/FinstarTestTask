CREATE OR REPLACE FUNCTION get_client_daily_payments(
    p_client_id bigint,
    p_date_from date,
    p_date_to   date
)
    RETURNS TABLE (pay_date date, total_amount numeric)
    LANGUAGE sql
    STABLE
AS $$
WITH days AS (
    SELECT gs::date AS d
    FROM generate_series(p_date_from::timestamp,
                         p_date_to::timestamp,
                         interval '1 day') AS gs
),
     sums AS (
         SELECT dt::date AS d, SUM(amount) AS s
         FROM clientpayments
         WHERE clientid = p_client_id
           AND dt >= p_date_from
           AND dt <  p_date_to + 1
         GROUP BY dt::date
     )
SELECT days.d, COALESCE(sums.s, 0)
FROM days
         LEFT JOIN sums USING (d)
ORDER BY days.d;
$$;