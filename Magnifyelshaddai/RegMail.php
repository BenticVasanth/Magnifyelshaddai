<?php
error_reporting(E_ALL);
ini_set('display_errors', 1);

echo "<h2>Mail Debug Started</h2><hr>";

if (!isset($_GET['action']) || $_GET['action'] != 'mail') {
    die("Invalid action.");
}

$mailType = isset($_GET['type']) ? trim($_GET['type']) : '';
$primaryId = isset($_GET['id']) ? trim($_GET['id']) : '';

echo "<b>Action:</b> " . $_GET['action'] . "<br>";
echo "<b>Mail Type:</b> " . $mailType . "<br>";
echo "<b>Primary ID:</b> " . $primaryId . "<br><br>";

if ($mailType == '' || $primaryId == '') {
    die("Mail Type or Primary ID is empty.");
}

/* SQL Server Connection */

$serverName = "serverName";
$connectionInfo = array(
    "Database" => "Database",
    "UID" => "UID",
    "PWD" => "PWD"
);

$conn = sqlsrv_connect($serverName, $connectionInfo);

if ($conn) {
    echo "<span style='color:green'>✓ Database Connected</span><br><br>";
} else {
    echo "<span style='color:red'>✗ Database Connection Failed</span><br>";
    die(print_r(sqlsrv_errors(), true));
}

/* Mail Templates */

$mailTemplate = array(
    'userlogin' => array(
        'subject' => 'Magnify El-Shaddai User Credentials',
        'body' =>
"Praise the LORD %USERFIRSTNAME%!

Username: %USERNAMEMAIL%
Password: %USERPASSWORD%

This is the confirmation email for your registration.

46. And Mary said, My soul doth magnify the Lord,
47. And my spirit hath rejoiced in God my Saviour. (Luke 1:46-47)

In Christ,
Bible Workshop Team."
    ),

    'userregister' => array(
        'subject' => 'Magnify El-Shaddai Registration Confirmation',
        'body' =>
"Praise the LORD %USERFIRSTNAME%,

Thank you for your registration.

In Christ,
Bible Workshop Team."
    )
);

$tableName = "Users";
$columnName = "UserId";

if ($mailType == "userregister") {
    $tableName = "RegistrationMaster";
    $columnName = "RMID";
}

$where = "";

if ($primaryId == "all") {
    $where = " AND IsActive = 1";
} else {
    $where = " AND $columnName = $primaryId";
}

$query = "SELECT * FROM $tableName WHERE 1=1 $where";

echo "<b>SQL Query:</b><br>";
echo $query . "<br><br>";

$result = sqlsrv_query($conn, $query);

if ($result === false) {
    echo "<span style='color:red'>SQL Query Failed</span><br>";
    die(print_r(sqlsrv_errors(), true));
}

if (!sqlsrv_has_rows($result)) {
    die("<span style='color:red'>No Records Found.</span>");
}

echo "<span style='color:green'>✓ Record Found</span><br><br>";

while ($row = sqlsrv_fetch_array($result, SQLSRV_FETCH_ASSOC)) {

    echo "<hr>";

    $to = ($mailType == 'userregister')
        ? trim($row['EmailId'])
        : trim($row['Email']);

    echo "<b>Name:</b> " . $row['Name'] . "<br>";
    echo "<b>Email:</b> " . $to . "<br>";

    if ($mailType != "userregister") {
        echo "<b>Password:</b> " . $row['Password'] . "<br>";
    }

    $mailBody = $mailTemplate[$mailType]['body'];

    $mailBody = str_replace("%USERFIRSTNAME%", $row['Name'], $mailBody);

    if ($mailType != "userregister") {
        $mailBody = str_replace("%USERNAMEMAIL%", $row['Email'], $mailBody);
        $mailBody = str_replace("%USERPASSWORD%", $row['Password'], $mailBody);
    }

    echo "<br><b>Subject:</b><br>";
    echo $mailTemplate[$mailType]['subject'];

    echo "<br><br><b>Mail Body:</b><br>";
    echo nl2br(htmlspecialchars($mailBody));

    /* Headers */

    $headers = "MIME-Version: 1.0\r\n";
    $headers .= "Content-type:text/plain;charset=UTF-8\r\n";
    $headers .= "From: bibleworkshop.chennai@gmail.com\r\n";
    $headers .= "Reply-To: bibleworkshop.chennai@gmail.com\r\n";
    $headers .= "Bcc: bibleworkshopteam@magnifyelshaddai.com\r\n";
    $headers .= "X-Mailer: PHP/" . phpversion();

    echo "<br><br><b>Headers:</b><br>";
    echo nl2br(htmlspecialchars($headers));

    echo "<br><br><b>Sending Mail...</b><br>";

    $status = mail(
        $to,
        $mailTemplate[$mailType]['subject'],
        $mailBody,
        $headers
    );

    if ($status) {
        echo "<span style='color:green;font-size:18px;'>✓ mail() returned TRUE</span><br>";
    } else {
        echo "<span style='color:red;font-size:18px;'>✗ mail() returned FALSE</span><br>";
        print_r(error_get_last());
    }
}

echo "<hr><h3>Debug Finished</h3>";
?>