$("#show-password-toggle").on('click', e => {
    try {
        let value = $(e.target).attr('value');
        $("#password")[0].type = value != "true" ? "text" : "password"
        $("#confirm-password")[0].type = value != "true" ? "text" : "password"
    } catch { }
})
$("#login-button").on('click', () => {
    let username = $("#username")[0].value;
    let password = $("#password")[0].value;
    if (username.length > 0 && password.length > 0) {
        Login(username, password)
    } else {
        error("Fields can NOT be left blank")
    }
})
$("#login-form input").on('keyup', e => {
    error("")
    if (e.key == "Enter") {
        e.target.blur();
        $("#login-button")[0].click();
    }
})
function error(message) {
    $(".error")[0].innerHTML = message;
}
async function Login(username, password) {
    error("")
    let data = new FormData();
    data.append('username', username);
    data.append('password', password);
    try {
        let response = await fetch('/api/auth/login', { method: "POST", body: data })
        if (response.ok) {
            let json = await response.json();
            if (!json.success) {
                error(json.message)
            } else {
                if ($("#remember-me-toggle").attr('value') == "true") {
                    document.cookie = `auth_email=${json.user.email}; expires=${new Date("3000").toUTCString()};path=/`
                    document.cookie = `auth_token=${json.user.token}; expires=${new Date("3000").toUTCString()};path=/`
                } else {
                    document.cookie = `auth_email=${json.user.email}; path=/`
                    document.cookie = `auth_token=${json.user.token}; path=/`
                }
                window.location.href = `/${ref}`;
            }
        } else
            error("Unknown Server Error")
    } catch {
    }
}