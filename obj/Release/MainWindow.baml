   M S B A M L   `   `   `  ���� G  CFile_manager, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:File_managerFile_manager      File_manager.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     ,localclr-namespace:File_manager       � Title$  Home server��$	��500��$	��600��$
��1080��$
��1920��.���  � ColumnDefinitions �� ��  � RowDefinitions �� $	��20* ��� $	��30* ��� $
��430* ��� $	��40* �.+�o� $	��Top=�$
��Left=�$��0��$��0��$��2��.@�m� $
��File��.@�m� -   $��
New window��m� -   $��Home directory��m� -   $��Close��m� $��
Connection��.@�m� -   $
��Info��m� -   $	��Set��m� -   $
��Save��m� $
��info��.@�m� -   $��Check for updates��m� -   $��Support��]� $��0��$��1��$��
10,5,10,-5q�.+��� $��15(�$��	10,0,10,0q�.��Local:�� -	   $��localWay��$��12(�$��1,1,0,1q�]� $��1��$��1��$��
10,5,10,-5q�.+��� $��15(�$��	10,0,10,0q�.��	Server:�� -
   $��	serverWay��$��12(�$��1,1,0,1q��� -   $��local��$��0��$��2��$��10q� � ContextMenu �� .@�m� -   $
��Copy��m� -   $
��Move��m� -   $��Delete��m� -   $��Make directory��m� -   $��	Make file��m� -   $��Refresh��
 � Tag$ local���� -   $��server��$��1��$��2��$��10q� �� .@�m� -   $
��Copy��m� -   $
��Move��m� -   $��Delete��m� -   $��Make directory��m� -   $��	Make file��m� -   $��Refresh��$ server��� $��	10,0,10,0q�$��30��$��3��$��0��$	��Top=�$��2�� �� $
��115* ��� $
��115* ��� $
��115* ��� $
��115* ��� $
��115* ��� $
��115* � �� .+��� -   $
��view��$��View(F3)��$��0��$��2q��� -   $
��edit��$��Edit(F4)��$��1��$��2q��� -   $
��copy��$��Copy(F5)��$��2��$��2q��� -   $
��move��$��Move(F6)��$��3��$��2q��� -   $��mkdir��$��	MkDir(F7)��$��4��$��2q��� -   $��delete��$��Delete(del)��$��5��$��2q�