   M S B A M L   `   `   `  ����  CopyDatabase!    CopyDatabase.SettingConnect   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  Connect��5   	   $	��350��6    � SizeToContent$ Height=�6%    � WindowStartupLocation$ CenterOwner=�6<    � ShowInTaskbar .   � 
ResizeMode$ NoResize=�6v   .��5      �� $��10q�6   .+�5   
   �� $��Fonte de dados de ORIGEM��6   $��5q�65   .��5	      �� .+�5
      �� $��
Instância��6   �� 5      -   5         ��Name$ txtInstancia��5      $��.\SQL2��60   �� 5      $��Database��6   �� 5      -   5        $ txtDatabase��5      �� 5      $��0,5,0,5q�6   $��0,10,0,0q�6-   ��5      �� 5      -   5         ��Name$	 chk��5   #   $��Autenticação��60   .��5      �� {�6!   ��  �� ElementName$	 chk�� �� Path$ 	IsChecked�.+�5      �� $
��User��6$   �� 5      -   5        $ txtUser��5   "   $��sa��63   �� 5      $��Password��6$   G� 5      -   5        	 G�Name$	 txtPwd��5   &   
 G� Password$
 sic742��66   5      5      5      5      �� 5   
   $��Fonte de dados de DESTINO��6   $��5q�66   .��5      �� .+�5      �� $��
Instância��6   �� 5       -   5        $ txtInstanciaDest��5       $��.\SQL1��64   �� 5!      $��Database��6   �� 5"      -   5        $ txtDatabaseDest��5"      �� 5$      $��0,5,0,5q�6   $��0,10,0,0q�6-   ��5%      �� 5&      -   5        $ chkDest��5&   #   $��Autenticação��64   .��5(      �� {�6!   �� $ chkDest��$ 	IsChecked�.+�5)      �� $
��User��6$   �� 5*      -	   5        $ txtUserDest��5*   "   $��sa��67   �� 5,      $��PasswordDest��6$   G� 5-      -
   5        $	 
txtPwdDest��5-   &   $
 sic742��6:   5.      5/      50      51      �� 52   
   -   5        $��Connect ...��52      $
��Auto��6'   $��0,10,0,0q�64   6F   53      54      