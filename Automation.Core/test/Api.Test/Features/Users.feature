Feature: Users

Scenario: Create User
	Given the user details as follows:
		| name | job     |
		| John | Finance |

Scenario: Open Chrome
	Given User open 'chromium' browser
	When I navigate to 'https://www.google.com/'
	Then 'https://www.google.com/' opens
