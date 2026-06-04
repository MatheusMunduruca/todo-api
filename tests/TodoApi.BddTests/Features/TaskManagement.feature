Feature: Gerenciamento de missoes na Taverna do Gregor
  Para organizar minhas tarefas
  Como um viajante autenticado
  Quero criar e concluir missoes

  Scenario: Criar uma nova missao
    Given que estou autenticado na taverna
    When eu peco uma missao chamada "Derrotar o dragao"
    Then a missao e criada com sucesso
    And a missao aparece com status "Pending"

  Scenario: Concluir uma missao
    Given que estou autenticado na taverna
    And eu tenho uma missao chamada "Coletar ervas raras"
    When eu concluo a missao
    Then a missao fica com status "Done"

  Scenario: Missoes exigem autenticacao
    Given que nao estou autenticado
    When eu listo minhas missoes
    Then o acesso e negado
