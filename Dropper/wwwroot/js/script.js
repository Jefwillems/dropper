window.addEventListener('load', () => {
    const feed = document.getElementById('feed');
    const input = document.getElementById('message');
    const sendBtn = document.getElementById('send');
    const themeToggle = document.getElementById('themeToggle');

    function applyTheme(theme) {
        document.documentElement.setAttribute('data-theme', theme);
        themeToggle.textContent = theme === 'dark' ? '☀' : '🌙';
        localStorage.setItem('dropper-theme', theme);
    }

    applyTheme(localStorage.getItem('dropper-theme') || 'dark');

    themeToggle.addEventListener('click', () => {
        const current = document.documentElement.getAttribute('data-theme');
        applyTheme(current === 'dark' ? 'light' : 'dark');
    });

    const source = new EventSource('/events');
    source.addEventListener('events', (e) => {
        const { content } = JSON.parse(e.data);
        prependCard(content);
    });

    function prependCard(text) {
        const card = document.createElement('div');
        card.className = 'card fresh';
        card.textContent = text;
        card.addEventListener('click', () => {
            navigator.clipboard.writeText(text).then(() => {
                card.classList.add('copied');
                setTimeout(() => card.classList.remove('copied'), 1500);
            });
        });
        feed.prepend(card);
        setTimeout(() => card.classList.remove('fresh'), 1200);
    }

    async function send() {
        const message = input.value.trim();
        if (!message) return;
        input.value = '';
        await fetch('/events', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ Id: crypto.randomUUID(), Content: message })
        });
    }

    sendBtn.addEventListener('click', send);
    input.addEventListener('keydown', (e) => { if (e.key === 'Enter') send(); });
});
