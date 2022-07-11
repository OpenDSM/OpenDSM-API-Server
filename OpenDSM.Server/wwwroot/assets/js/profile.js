$("#logout-btn").on('click', () => {
    document.cookie = `auth_token=null; expires=${new Date("2000").toUTCString()};path=/`
    document.cookie = `auth_username=null; expires=${new Date("2000").toUTCString()};path=/`
    window.location.href = "/"
})