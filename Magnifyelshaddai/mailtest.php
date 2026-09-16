<?php

if(!isset($_GET['action']) && $_GET['action'] != 'mail'){
	return false;
}

$mailType = isset($_GET['type']) ? trim($_GET['type']) : '';
$primaryId = isset($_GET['id']) ? trim($_GET['id']) : '';

if($mailType == '' || $primaryId == ''){
	return false;
}

$serverName = "SG2NWPLS19SQL-v08.mssql.shr.prod.sin2.secureserver.net";
$connectionInfo = array( "Database"=>"Elshaddai", "UID"=>"JesusMary", "PWD"=>"Elohim_1927");
$conn = sqlsrv_connect( $serverName, $connectionInfo);

if( $conn ) {
     echo "Connection established.<br />";
}else{
     echo "Connection could not be established.<br />";
     die( print_r( sqlsrv_errors(), true));
}

$mailTemplate = array(
	'userlogin' => array(
		'subject' => 'Magnify El-Shaddai User Credentials',
		'body' => "Praise the LORD %USERFIRSTNAME%!\r\n\r\nUsername: %USERNAMEMAIL%\r\nPassword: %USERPASSWORD%\r\n\r\nThis is the confirmation email for your registration.\r\n\r\n46.And Mary said, My soul doth magnify the Lord, \r\n47. And my spirit hath rejoiced in God my Saviour. (Luke:1:46-47.)\r\n\r\nIn Christ,\r\nBible workshop team."
	),
	'userregister' => array(
		'subject' => 'Magnify El-Shaddai registration confirmation email',
		'body' => "Praise the LORD %USERFIRSTNAME%,\r\n\r\nThank you for your Registration to participate in Bible workshop meeting.\r\n\r\n12. Now during those days he went out to the mountain to pray; and he spent the night in prayer to God. (Luke: 6:12)\r\n\r\nContact Number:-\r\nBro. Albert: +91-9444306330,\r\nBro. Benatic: +91-7373843646\r\n\r\nIn Christ,\r\nBible Workshop Team."
	)
);

$tableName = 'Users';
$columnName = 'UserId';

if($mailType == 'userregister') {
	$tableName = 'RegistrationMaster';
	$columnName = 'RMID';
}

$where = '';
if($columnName != '') {
	$where .= " AND ".$columnName." = ".$primaryId;
}

if($primaryId == 'all') {
	$where = " AND IsActive = 1";
}

$headers = "From: bibleworkshop.chennai@gmail.com\r\n";
$headers .= "Reply-To: bibleworkshop.chennai@gmail.com\r\n";
$headers .= "Bcc: bibleworkshopteam@magnifyelshaddai.com\r\n";
$headers .= "X-Mailer: PHP/" . phpversion();

$query = "SELECT * FROM ".$tableName." WHERE 1=1 ".$where;
$result = sqlsrv_query($conn, $query);
while($row = sqlsrv_fetch_array($result, SQLSRV_FETCH_ASSOC)) {
	// $to = "benaticgrace@gmail.com, benaticvasanth@magnifyelshaddai.com, mcharles3321@gmail.com, bibleworkshopteam@magnifyelshaddai.com, edwinamburose@outlook.com, benatic_vasanth@cms.co.in, charlesjoseph@magnifyelshaddai.com";
	$to = $mailType == 'userregister' ? trim($row['EmailId']) : trim($row['Email']);
	$mailBody = $mailTemplate[$mailType]['body'];

	$mailBody = str_replace('%USERFIRSTNAME%', $row['Name'], $mailBody);
	if($mailType != 'userregister') {
		$mailBody = str_replace('%USERNAMEMAIL%', $row['Email'], $mailBody);
		$mailBody = str_replace('%USERPASSWORD%', $row['Password'], $mailBody);
	}

	try {
		if(@mail($to, $mailTemplate[$mailType]['subject'], $mailBody, $headers)) {
			echo 'Mail Sent...';
			// return true;
		} else {
			echo 'Mail not sent....';
			// return false;
		}
	} catch(Exception $e) {
		echo 'Message: ' .$e->getMessage();
	}
}
?>