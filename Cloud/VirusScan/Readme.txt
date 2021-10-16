Test Cases
	- Some service send a file
		1. File need to upload shared storage
		2. Send event with the file and the metadata
		3. Receive the event from bus
		4. Save the ticket in database with the time for scan
		5. Scan the file
		6. If scanning sucess mark with the result (virus or not)
		7. If the file has virus increment in one the amout of file with virus asociate to the customer.
		7. If error in the scanning rescheduler and go step 4
		
	- Customer upload file over scan service
		1. Save the file in the virus scan service and mark as unconfirmed, set time out for expiration
		3. Confirm the file will be in use over the bus and mark the file as confirmed
		2. Go to the step 4.
	- Customer upload file over other service
		1. Same as step 1. Just verify the customer virus attemp has not grather than the allowed