# Gabriel.Cat.BaseDeDades

Necesidades a cumplir:
  Seguridad,Sin SQL para los de fuera,multiplesBD,Seguimiento datos,Simple y Optimo (que no cargue la ram si no es necesario)

Puntos a Hacer:
  Seguridad usando Gabriel.Cat.Seguretat
  Cargar lo minimo para no cargar la memoria RAM.
  Diferentes niveles de cifrado -> Generico,Tabla,Columna
  Quitar toda la parte SQL en la medida de lo posible.
  Hacer conectores para todas las BD que necesite hacer compatible: MySQL,Access,SQLite,etc...
  Permitir actualizar objetos(si desarrollan cambios que puedan convertir lo anterior en lo nuevo de forma sencilla)
  Permitir cambiar la clave de cifrado a todos los niveles
  Poder hacer un seguimiento de los campos (si la BD se actualiza de forma externa poder conseguir los nuevos cambios)
  hacer clase para que un objeto se guarde y se actualize en varias BD a la vez(asi se puede hacer backups facilmente)
  Excepciones que ayuden a entender el origen y el problema.
  Permitir personalización (si el programador quiere que la parte SQL vaya distinta que pueda)
  Abtraer al maximo para que los conectores sean lo mas simples de implementar :D
  Poder descargar los objetos de la ram y que se queden con el minimo.
  
  
  
  
