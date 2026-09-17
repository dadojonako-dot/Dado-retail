import 'package:flutter/material.dart';

void main() => runApp(const DadoApp());

class DadoApp extends StatelessWidget {
  const DadoApp({super.key});
  @override
  Widget build(BuildContext context) => MaterialApp(
        debugShowCheckedModeBanner: false,
        title: 'ДАДО',
        theme: ThemeData(
          colorScheme: ColorScheme.fromSeed(seedColor: const Color(0xFFE30613)),
          useMaterial3: true,
          scaffoldBackgroundColor: const Color(0xFFF8F8F8),
        ),
        home: const LoginScreen(),
      );
}

class LoginScreen extends StatelessWidget {
  const LoginScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        body: SafeArea(
          child: Padding(
            padding: const EdgeInsets.all(24),
            child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
              const Spacer(),
              const Icon(Icons.shopping_bag_rounded, size: 72, color: Color(0xFFE30613)),
              const SizedBox(height: 12),
              const Text('ДАДО', textAlign: TextAlign.center, style: TextStyle(fontSize: 42, fontWeight: FontWeight.w900, color: Color(0xFFE30613))),
              const Text('Одежда и 1000 мелочей', textAlign: TextAlign.center),
              const Spacer(),
              const Text('Вход в аккаунт', style: TextStyle(fontSize: 26, fontWeight: FontWeight.bold)),
              const SizedBox(height: 12),
              const TextField(keyboardType: TextInputType.phone, decoration: InputDecoration(labelText: 'Номер телефона', prefixText: '+992 ', border: OutlineInputBorder())),
              const SizedBox(height: 12),
              FilledButton(onPressed: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const OtpScreen())), child: const Text('Получить код')),
              const Spacer(),
            ]),
          ),
        ),
      );
}

class OtpScreen extends StatelessWidget {
  const OtpScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(),
        body: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
            const Icon(Icons.verified_user_outlined, size: 64, color: Color(0xFFE30613)),
            const SizedBox(height: 24),
            const Text('Подтверждение входа', style: TextStyle(fontSize: 26, fontWeight: FontWeight.bold)),
            const SizedBox(height: 8),
            const Text('Введите одноразовый код. Это второй уровень авторизации.'),
            const SizedBox(height: 20),
            const TextField(keyboardType: TextInputType.number, maxLength: 6, decoration: InputDecoration(labelText: '6-значный код', border: OutlineInputBorder())),
            FilledButton(onPressed: () => Navigator.pushAndRemoveUntil(context, MaterialPageRoute(builder: (_) => const HomeShell()), (_) => false), child: const Text('Подтвердить')),
          ]),
        ),
      );
}

class HomeShell extends StatefulWidget {
  const HomeShell({super.key});
  @override State<HomeShell> createState() => _HomeShellState();
}

class _HomeShellState extends State<HomeShell> {
  int index = 0;
  final pages = const [StoreHome(), CatalogScreen(), CartScreen(), OrdersScreen(), ProfileScreen()];
  @override
  Widget build(BuildContext context) => Scaffold(
        body: pages[index],
        bottomNavigationBar: NavigationBar(
          selectedIndex: index,
          onDestinationSelected: (v) => setState(() => index = v),
          destinations: const [
            NavigationDestination(icon: Icon(Icons.home_outlined), selectedIcon: Icon(Icons.home), label: 'Главная'),
            NavigationDestination(icon: Icon(Icons.grid_view), label: 'Каталог'),
            NavigationDestination(icon: Icon(Icons.shopping_cart_outlined), label: 'Корзина'),
            NavigationDestination(icon: Icon(Icons.receipt_long_outlined), label: 'Заказы'),
            NavigationDestination(icon: Icon(Icons.person_outline), label: 'Профиль'),
          ],
        ),
      );
}

class StoreHome extends StatelessWidget {
  const StoreHome({super.key});
  @override Widget build(BuildContext context) => SafeArea(child: ListView(padding: const EdgeInsets.all(16), children: [
    const Text('ДАДО', style: TextStyle(fontSize: 30, fontWeight: FontWeight.w900, color: Color(0xFFE30613))),
    const SizedBox(height: 12),
    const TextField(decoration: InputDecoration(hintText: 'Поиск одежды, обуви, товаров для дома…', prefixIcon: Icon(Icons.search), border: OutlineInputBorder())),
    const SizedBox(height: 18),
    Card(child: Padding(padding: const EdgeInsets.all(20), child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
      const Text('Новая коллекция', style: TextStyle(fontSize: 24, fontWeight: FontWeight.bold)),
      const Text('Одежда, аксессуары и 1000 мелочей'),
      const SizedBox(height: 12), FilledButton(onPressed: () {}, child: const Text('Смотреть товары')),
    ]))),
    const SizedBox(height: 20),
    const Text('Категории', style: TextStyle(fontSize: 22, fontWeight: FontWeight.bold)),
    const Wrap(spacing: 8, runSpacing: 8, children: [Chip(label: Text('Женская одежда')), Chip(label: Text('Мужская одежда')), Chip(label: Text('Детская одежда')), Chip(label: Text('Обувь')), Chip(label: Text('Для дома')), Chip(label: Text('1000 мелочей'))]),
  ]));
}

class CatalogScreen extends StatelessWidget { const CatalogScreen({super.key}); @override Widget build(BuildContext context) => const SimplePage(title: 'Каталог', body: 'Одежда • Обувь • Аксессуары • Для дома • 1000 мелочей'); }
class CartScreen extends StatelessWidget { const CartScreen({super.key}); @override Widget build(BuildContext context) => const SimplePage(title: 'Корзина', body: 'Товары, промокод, бонусы, доставка и оформление заказа'); }
class OrdersScreen extends StatelessWidget { const OrdersScreen({super.key}); @override Widget build(BuildContext context) => const SimplePage(title: 'Мои заказы', body: 'Текущие заказы, история и отслеживание доставки'); }

class ProfileScreen extends StatelessWidget {
  const ProfileScreen({super.key});
  @override Widget build(BuildContext context) => SafeArea(child: ListView(padding: const EdgeInsets.all(16), children: [
    const Text('Личный кабинет', style: TextStyle(fontSize: 28, fontWeight: FontWeight.bold)),
    const SizedBox(height: 16),
    Card(child: ListTile(leading: const CircleAvatar(child: Icon(Icons.account_balance_wallet)), title: const Text('ДАДО Wallet'), subtitle: const Text('Баланс: 1 250,00 с.'), trailing: const Icon(Icons.chevron_right))),
    const ListTile(leading: Icon(Icons.receipt_long), title: Text('Мои заказы')),
    const ListTile(leading: Icon(Icons.credit_card), title: Text('Способы оплаты')),
    const ListTile(leading: Icon(Icons.card_giftcard), title: Text('Бонусы')),
    const ListTile(leading: Icon(Icons.favorite_border), title: Text('Избранное')),
    const ListTile(leading: Icon(Icons.security), title: Text('Безопасность и 2FA')),
  ]));
}

class SimplePage extends StatelessWidget {
  final String title; final String body;
  const SimplePage({super.key, required this.title, required this.body});
  @override Widget build(BuildContext context) => SafeArea(child: Padding(padding: const EdgeInsets.all(20), child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [Text(title, style: const TextStyle(fontSize: 28, fontWeight: FontWeight.bold)), const SizedBox(height: 20), Text(body, style: const TextStyle(fontSize: 17))])));
}
