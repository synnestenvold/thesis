clc;

total = [169,321,582,1187,2545,5487,10344];
k = [67	90	103	143	180	246	379];
bcLoad = [8	14	23	38	81	91	205];
invertK = [43	137	307	888	2072	4851	9516];
uSS = [11	14	22	28	55	55	66];

data = [total; k; bcLoad; invertK; uSS]

A = 'BC+Load';
k = '\delta, \epsilon and \sigma';

names = {'FEMSolver', 'Create K', 'Apply', 'Invert K', 'Calculate'};


figure('Renderer', 'painters', 'Position', [10 10 900 500])
hold on
b = bar(data,'FaceColor','flat');

size(data,2)

for k = 1:size(data,2)
    b(k).CData = (size(data,2))-k;
end

set(gca,'xtick',[1:5],'xticklabel',names);

set(gca, 'FontName', 'Times New Roman')
set(gca,'fontsize',14)
ylabel('Time[ms]');

labels=['1029 dofs';'1536 dofs'; '2187 dofs'; '3000 dofs'; '3993 dofs';'5184 dofs';'6591 dofs'];
legend(b,labels);

numbersToAdd = [1029	1536	2187	3000	3993	5184	6591;
1029	1536	2187	3000	3993	5184	6591;
1029	1536	2187	3000	3993	5184	6591;
1029	1536	2187	3000	3993	5184	6591;
1029	1536	2187	3000	3993	5184	6591];
                
barWidth = b.BarWidth;
numCol = size(data,2);
cnt = 0;
for ii = numbersToAdd'
    cnt = cnt + 1;
    xPos = linspace(cnt - barWidth/2, cnt + barWidth / 2, numCol+1);
    idx = 1;
    for jj = xPos(1:end-1)
        val = numbersToAdd(cnt,idx);
        y = data(cnt,idx);
        %text(jj+barWidth/7, y + 30, num2str(val),'Rotation',90,'HorizontalAlignment','left','VerticalAlignment','bottom');
        idx = idx +1;
    end     
end


