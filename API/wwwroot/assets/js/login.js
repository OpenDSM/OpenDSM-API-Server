(() => {
    let submit = $("button.btn")[0];
    submit.disabled = true;
    $("button").on('click', async () => {
        let data = new FormData();
        data.append('username', $("#email-input")[0].value)
        data.append('password', $("#password-input")[0].value)
        let response = await fetch('/api/auth/login', { method: "POST", body: data });
        let json = await response.json();
        console.log(json)
        if (!json.success) {
            $(".error")[0].innerText = `Unable to Login: ${json.reason}`
        } else {
            $(".error")[0].innerText = "";
            document.cookie = `auth_token=${json.token}; expires=${new Date("3000").toUTCString()};path=/`
            document.cookie = `auth_username=${json.username}; expires=${new Date("3000").toUTCString()};path=/`
            window.location.href = "/";
        }
    })
    $("input").on('keyup', e => {
        if (e.key == "Enter") {
            submit.click();
        } else {
            let index = 0;
            Array.from($("input")).forEach(e => {
                if (e.value != "") {
                    index++;
                }
            })
            if ($("input").length == index) {
                submit.disabled = false;
            }
        }
    })
}).call();