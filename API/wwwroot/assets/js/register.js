(() => {
    let submit = $("button.btn")[0];
    submit.disabled = true;
    $("button").on('click', async () => {
        let data = new FormData();
        data.append('email', $("#email-input")[0].value)
        data.append('username', $("#username-input")[0].value)
        data.append('password', $("#password-input")[0].value)
        let response = await fetch('/api/auth/register', { method: "POST", body: data });
        let json = await response.json();
        if (!json.success) {
            error(`Unable to Register: ${json.reason}`)
        } else {
            $(".error")[0].innerText = "";
            document.cookie = `auth_token=${json.token}; expires=${new Date("3000").toUTCString()};path=/`
            document.cookie = `auth_username=${json.username}; expires=${new Date("3000").toUTCString()};path=/`
            window.location.href = "/";
        }
    })
    $("input").on('keyup', e => {
        error('')
        submit.disabled = !Check();
        if (e.key == "Enter") {
            submit.click();
        }
    })

    function Check() {
        let index = 0;
        Array.from($("input")).forEach(e => {
            if (e.value != "") {
                index++;
            }
        })
        return CheckEmail() && CheckPassword() && $("input").length == index;
    }

    function CheckPassword() {
        let pwd = $("#password-input")[0].value
        let cpwd = $("#confirm-password-input")[0].value
        if (pwd != "" && cpwd != "") {
            if (pwd != cpwd) {
                error("Passwords Need to Match")
                return false;
            } else {
                error('')
                return true;
            }
        } else if (pwd != "" && cpwd == "") {
            let length = 6;
            if ($("#password-input")[0].value.length < length) {
                error(`Password must be at least ${length} characters long`)
                return false
            }
        }
        return false;
    }

    function CheckEmail() {
        let value = $("#email-input")[0].value;
        let halfs = value.split('@');
        if (!(value.includes('@') && halfs.length == 2 && halfs[0] != "" && halfs[1] != "")) {
            error("Invalid Email Format: name@example.com")
            return false;
        }
        error('')
        return true;
    }

    function error(error) {
        $(".error")[0].innerText = error;
    }
}).call();