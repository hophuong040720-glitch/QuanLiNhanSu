// ==========================================
// 1. GIAO DIỆN DARK/LIGHT MODE
// ==========================================
document.addEventListener('DOMContentLoaded', () => {
    const themeToggle = document.getElementById('theme-toggle');
    if (!themeToggle) return;

    const icon = themeToggle.querySelector('i');
    
    // Lấy state từ localStorage
    const savedTheme = localStorage.getItem('theme');
    if (savedTheme === 'dark') {
        document.documentElement.setAttribute('data-theme', 'dark');
        icon.classList.replace('bi-moon-stars-fill', 'bi-brightness-high-fill');
        icon.classList.replace('text-dark', 'text-warning');
    }

    // Sự kiện click toggle
    themeToggle.addEventListener('click', () => {
        let currentTheme = document.documentElement.getAttribute('data-theme');
        let newTheme = currentTheme === 'dark' ? 'light' : 'dark';
        
        document.documentElement.setAttribute('data-theme', newTheme);
        localStorage.setItem('theme', newTheme);

        if (newTheme === 'dark') {
            icon.classList.replace('bi-moon-stars-fill', 'bi-brightness-high-fill');
            icon.classList.replace('text-dark', 'text-warning');
        } else {
            icon.classList.replace('bi-brightness-high-fill', 'bi-moon-stars-fill');
            icon.classList.replace('text-warning', 'text-dark');
        }
    });

    // ==========================================
    // 2. SIDEBAR TOGGLE
    // ==========================================
    const sidebarToggle = document.getElementById('sidebar-toggle');
    const sidebar = document.getElementById('sidebar');
    const content = document.getElementById('content');

    if (sidebarToggle && sidebar && content) {
        sidebarToggle.addEventListener('click', () => {
            sidebar.classList.toggle('collapsed');
            content.classList.toggle('sidebar-collapsed');
        });

        // Tự động thu gọn sidebar trên màn hình nhỏ
        if (window.innerWidth < 992) {
            sidebar.classList.add('collapsed');
            content.classList.add('sidebar-collapsed');
        }
    }

    // ==========================================
    // 3. ANIMATION CHO NỘI DUNG MỚI LOAD
    // ==========================================
    const cards = document.querySelectorAll('.card');
    cards.forEach((card, index) => {
        card.classList.add('fade-in-up');
        card.style.animationDelay = `${index * 0.05}s`;
    });
});
