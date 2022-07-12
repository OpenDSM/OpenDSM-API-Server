$("#show-password-toggle").on('click', e => {
    try {
        let value = $(e.target).attr('value');
        $("#password")[0].type = value != "true" ? "text" : "password"
        $("#confirm-password")[0].type = value != "true" ? "text" : "password"
    } catch { }
})

$("#signup-button").on('click', () => {
    let email = $("#signup-form #email")[0].value;
    let username = $("#signup-form #username")[0].value;
    let password = $("#signup-form #password")[0].value;

    Signup(email, username, password)
})

$("#signup-form input").on('keyup', e => {
    error("")
    switch (e.key) {
        case "Enter":
            e.target.blur()
            $("#signup-button")[0].click();
            break;
        default:
            $("#signup-button")[0].disabled = !(CheckEmail() && CheckUsername() && CheckPasswordLength() && CheckPasswordStrength() && CheckConfirmPassword())
            break;
    }
})
$("#signup-form input#username").on('keyup', e => {
    if (!CheckUsername()) {
        error("Username must be at least 6 characters long")
    }
})
$("#signup-form input#email").on('keyup', e => {
    if (!CheckEmail()) {
        error("Email must be in a name@example.com format")
    }
})
$("#signup-form input#password").on('keyup', e => {
    if (!CheckPasswordLength()) {
        error("Password must be at least 8 characters long")
    } else if (!CheckPasswordStrength()) {
        error("Password must include at least 1 Uppercase, 1 Lowercase, 1 Number, and 1 Special character")
    }
})
$("#signup-form input#confirm-password").on('keyup', e => {
    if (!CheckConfirmPassword()) {
        error("Passwords must match")
    }
})

function CheckUsername() {
    return $("#username")[0].value.length > 6
}
function CheckEmail() {
    try {
        let value = $("#email")[0].value
        let sides = value.split('@')
        let dot = sides[1].split('.')
        return value.includes('@') && sides.length == 2 && sides[0].length > 0 && sides[1].length >= 2 && sides[1].includes('.') && dot.length == 2 && dot[0].length > 0 && dot[1].length > 0
    } catch {
        return false;
    }
}
function CheckPasswordLength() {
    return $("#password")[0].value.length >= 8;
}
function CheckPasswordStrength() {
    let strongRegex = new RegExp("^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#\$%\^&\*])(?=.{8,})");
    return strongRegex.test($("#password")[0].value);
}
function CheckConfirmPassword() {
    return $("#password")[0].value == $("#confirm-password")[0].value;
}
function error(message) {
    $(".error")[0].innerHTML = message;
}
async function Signup(email, username, password) {
    error("")
    let data = new FormData();
    data.append('username', username);
    data.append('email', email);
    data.append('password', password);
    try {
        let response = await fetch('/api/auth/signup', { method: "POST", body: data })
        if (response.ok) {
            let json = await response.json();
            if (!json.success) {
                error(json.message)
            } else {
                let popup = new Popup("verify-account");
                popup.open();

                document.cookie = `auth_email=${json.user.email}; path=/`
                document.cookie = `auth_token=${json.user.token}; path=/`
            }
        } else
            error("Unknown Server Error")
    } catch {
    }
}