window.themeManager = {
    getTheme: function () {
        return document.documentElement.getAttribute('data-bs-theme') || 'dark';
    },

    setTheme: function (theme) {
        document.documentElement.setAttribute('data-bs-theme', theme);
        localStorage.setItem('theme', theme);
    },

    toggleTheme: function () {
        const currentTheme = this.getTheme();
        const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
        this.setTheme(newTheme);
        return newTheme;
    },

    initialize: function () {
        const savedTheme = localStorage.getItem('theme') || 'dark';
        this.setTheme(savedTheme);
        return savedTheme;
    }
};