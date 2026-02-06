using NUnit.Framework;
using App;

namespace Tests {
    public class UtilsTests {
        [Test]
        public void CheckEmail_ValidEmails_ReturnNull() {
            Assert.IsNull(Utils.CheckEmail("test@example.com"));
            Assert.IsNull(Utils.CheckEmail("user.name@domain.co.uk"));
            Assert.IsNull(Utils.CheckEmail("user+tag@example.com"));
            Assert.IsNull(Utils.CheckEmail("123@123.com"));
        }

        [Test]
        public void CheckEmail_InvalidEmails_ReturnErrorMessage() {
            Assert.IsNotNull(Utils.CheckEmail("plainaddress"));
            Assert.IsNotNull(Utils.CheckEmail("@missingusername.com"));
            Assert.IsNotNull(Utils.CheckEmail("username@.com"));
            Assert.IsNotNull(Utils.CheckEmail("username@domain"));
            Assert.IsNotNull(Utils.CheckEmail(null));
            Assert.IsNotNull(Utils.CheckEmail(""));
        }

        [Test]
        public void CheckEmail_MaliciousInputs_ReturnErrorMessage() {
            // The bug we fixed: partial matching allowed suffix
            Assert.IsNotNull(Utils.CheckEmail("valid@email.com<script>alert(1)</script>"));
            Assert.IsNotNull(Utils.CheckEmail("<script>alert(1)</script>valid@email.com"));
            Assert.IsNotNull(Utils.CheckEmail("valid@email.com\nnewline"));
        }

        [Test]
        public void CheckEmail_CaseInsensitivity() {
            Assert.IsNull(Utils.CheckEmail("User@Test.com"));
            Assert.IsNull(Utils.CheckEmail("USER@TEST.COM"));
        }

        [Test]
        public void CheckUsernameAndPassword_ValidInputs_ReturnNull() {
            Assert.IsNull(Utils.CheckUsernameAndPassword("admin123", "password123"));
            Assert.IsNull(Utils.CheckUsernameAndPassword("playerOne", "securePass"));
        }

        [Test]
        public void CheckUsernameAndPassword_InvalidInputs_ReturnErrorMessage() {
            // Length checks (6-20)
            Assert.IsNotNull(Utils.CheckUsernameAndPassword("short", "password123"));
            Assert.IsNotNull(Utils.CheckUsernameAndPassword("admin123", "short"));
            Assert.IsNotNull(Utils.CheckUsernameAndPassword("waytoolongusername1234567890", "password123"));

            // Null/Empty
            Assert.IsNotNull(Utils.CheckUsernameAndPassword(null, "password123"));
            Assert.IsNotNull(Utils.CheckUsernameAndPassword("admin123", null));
            Assert.IsNotNull(Utils.CheckUsernameAndPassword("", ""));

            // Invalid chars
            Assert.IsNotNull(Utils.CheckUsernameAndPassword("admin@123", "password123")); // username allows alnum only
            Assert.IsNotNull(Utils.CheckUsernameAndPassword("admin123", "pass word")); // password no spaces
        }

        [Test]
        public void RedactSensitiveData_RedactsNonStrings() {
            var json = "{\"token\": 12345, \"secret\": \"s3cr3t\", \"key\": true}";
            var redacted = Utils.RedactSensitiveData(json);

            Assert.IsTrue(redacted.Contains("\"token\": \"[REDACTED]\""));
            Assert.IsTrue(redacted.Contains("\"secret\": \"[REDACTED]\""));
            Assert.IsTrue(redacted.Contains("\"key\": \"[REDACTED]\""));
        }

        [Test]
        public void RedactSensitiveData_PreservesStructure() {
            var json = "{\"token\": {\"nested\": 1}, \"secret\": [1, 2], \"other\": \"value\"}";
            var redacted = Utils.RedactSensitiveData(json);

            // Should NOT redact complex objects to avoid breaking JSON structure
            Assert.IsTrue(redacted.Contains("\"token\": {\"nested\": 1}"));
            Assert.IsTrue(redacted.Contains("\"secret\": [1, 2]"));
            Assert.IsTrue(redacted.Contains("\"other\": \"value\""));
        }
    }
}
